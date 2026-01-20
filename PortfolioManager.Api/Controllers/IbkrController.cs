using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using PortfolioManager.Core.Services;
using PortfolioManager.Contracts.Models;
using PortfolioManager.Contracts.Web;

namespace PortfolioManager.Api.Controllers;

[ApiController]
[Route("api/ibkr")]
[EnableRateLimiting("auth")]
public class IbkrController : ControllerBase
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<IbkrController> _logger;
    private readonly IIbkrSessionManager _sessionManager;
    private readonly IIbkrClient _ibkrClient;
    private static readonly TimeSpan SessionExpiration = TimeSpan.FromMinutes(5);

    public IbkrController(
        IMemoryCache cache, 
        ILogger<IbkrController> logger,
        IIbkrSessionManager sessionManager,
        IIbkrClient ibkrClient)
    {
        _cache = cache;
        _logger = logger;
        _sessionManager = sessionManager;
        _ibkrClient = ibkrClient;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartLogin([FromBody] IbkrLoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        {
            return BadRequest(new { message = "Username and password are required" });
        }

        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            // Launch Playwright automation for IBKR QR login
            var automation = new Services.IbkrQrLoginAutomation();
            var result = await automation.StartLoginAsync(req.Username, req.Password);
            
            // Generate secure session ID
            var secureSessionId = GenerateSecureSessionId();
            
            var sessionData = new SessionData
            {
                SessionId = secureSessionId,
                QrImage = result.QrImage,
                Authenticated = result.Authenticated,
                SessionCookies = result.SessionCookies,
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress,
                OriginalSessionId = result.SessionId
            };
            
            // Store in cache with sliding expiration
            _cache.Set(secureSessionId, sessionData, new MemoryCacheEntryOptions
            {
                SlidingExpiration = SessionExpiration,
                PostEvictionCallbacks =
                {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = (key, value, reason, state) =>
                        {
                            _logger.LogInformation("Session {SessionId} evicted. Reason: {Reason}", key, reason);
                        }
                    }
                }
            });
            
            return Ok(new { sessionId = secureSessionId, qrImageBase64 = Convert.ToBase64String(result.QrImage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting IBKR login");
            return StatusCode(500, new { message = "Failed to start login process" });
        }
    }

    [HttpGet("status/{sessionId}")]
    public IActionResult GetStatus(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest(new { message = "Session ID is required" });
        }

        // Validate session ownership by IP
        var currentIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (_cache.TryGetValue<SessionData>(sessionId, out var data))
        {
            // Verify IP matches
            if (data.IpAddress != currentIp)
            {
                _logger.LogWarning("Session access from different IP. Session: {SessionId}, Expected: {ExpectedIp}, Actual: {ActualIp}",
                    sessionId, data.IpAddress, currentIp);
                return Unauthorized(new { message = "Invalid session" });
            }
            
            return Ok(new { authenticated = data.Authenticated });
        }
        
        return NotFound(new { message = "Session not found or expired" });
    }

    [HttpPost("complete")]
    public IActionResult Complete([FromBody] IbkrQrLoginCompleteRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SessionId))
        {
            return BadRequest(new { message = "Session ID is required" });
        }

        // Validate session ownership by IP
        var currentIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (_cache.TryGetValue<SessionData>(req.SessionId, out var data))
        {
            // Verify IP matches
            if (data.IpAddress != currentIp)
            {
                _logger.LogWarning("Session completion from different IP. Session: {SessionId}, Expected: {ExpectedIp}, Actual: {ActualIp}",
                    req.SessionId, data.IpAddress, currentIp);
                return Unauthorized(new { message = "Invalid session" });
            }
            
            data.Authenticated = true;
            data.SessionCookies = req.SessionCookies;
            
            // Update cache with extended expiration after successful auth
            _cache.Set(req.SessionId, data, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
            
            return Ok();
        }
        
        return NotFound(new { message = "Session not found or expired" });
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts()
    {
        try
        {
            // Get username from custom header
            if (!Request.Headers.TryGetValue("X-IBKR-Username", out var usernameHeader))
            {
                return Unauthorized(new { message = "IBKR username not provided" });
            }

            var username = usernameHeader.ToString();
            
            // Get session cookies from session manager
            var cookieContainer = _sessionManager.GetSessionCookies(username);
            if (cookieContainer == null)
            {
                return Unauthorized(new { message = "IBKR session not found. Please authenticate first." });
            }

            // Pass username to IbkrClient for cookie injection via IbkrSessionHandler
            var accounts = await _ibkrClient.GetAccountsAsync(username);
            
            if (accounts == null)
            {
                return StatusCode(500, new { message = "Failed to retrieve accounts from IBKR" });
            }

            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving IBKR accounts");
            return StatusCode(500, new { message = "Failed to retrieve accounts" });
        }
    }

    [HttpGet("portfolio/{accountId}")]
    public async Task<IActionResult> GetPortfolio(string accountId)
    {
        try
        {
            if (!Request.Headers.TryGetValue("X-IBKR-Username", out var usernameHeader))
            {
                return Unauthorized(new { message = "IBKR username not provided" });
            }

            var username = usernameHeader.ToString();
            
            var cookieContainer = _sessionManager.GetSessionCookies(username);
            if (cookieContainer == null)
            {
                return Unauthorized(new { message = "IBKR session not found. Please authenticate first." });
            }

            var portfolio = await _ibkrClient.GetPortfolioSummaryAsync(accountId, username);
            
            if (portfolio == null)
            {
                return StatusCode(500, new { message = "Failed to retrieve portfolio from IBKR" });
            }

            return Ok(portfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving IBKR portfolio for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Failed to retrieve portfolio" });
        }
    }

    [HttpGet("positions/{accountId}")]
    public async Task<IActionResult> GetPositions(string accountId)
    {
        try
        {
            if (!Request.Headers.TryGetValue("X-IBKR-Username", out var usernameHeader))
            {
                return Unauthorized(new { message = "IBKR username not provided" });
            }

            var username = usernameHeader.ToString();
            
            var cookieContainer = _sessionManager.GetSessionCookies(username);
            if (cookieContainer == null)
            {
                return Unauthorized(new { message = "IBKR session not found. Please authenticate first." });
            }

            var positions = await _ibkrClient.GetPositionsAsync(accountId, username);
            
            if (positions == null)
            {
                return StatusCode(500, new { message = "Failed to retrieve positions from IBKR" });
            }

            // Map IbkrPosition to InstrumentDto
            var instrumentDtos = positions.Select(p => new InstrumentDto
            {
                Id = p.ConId?.ToString(),
                Symbol = p.Ticker ?? p.ContractDesc,
                Name = p.ContractDesc,
                Currency = p.Currency,
                BrokerageType = 1, // 1 = IBKR, adjust as needed
                SharesOwned = p.Position ?? 0,
                SharePrice = p.MktPrice ?? 0,
                InvestmentValue = p.MktValue ?? 0,
                CostBasis = p.AvgCost ?? 0,
                TotalReturn = p.RealizedPnl ?? 0,
                SimpleReturn = p.UnrealizedPnl ?? 0,
                DividendsReceived = 0 // Not available from IBKRPosition
            }).ToList();

            return Ok(instrumentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving IBKR positions for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Failed to retrieve positions" });
        }
    }

    private static string GenerateSecureSessionId()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private class SessionData
    {
        public string SessionId { get; set; } = string.Empty;
        public byte[] QrImage { get; set; } = Array.Empty<byte>();
        public bool Authenticated { get; set; }
        public object? SessionCookies { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string OriginalSessionId { get; set; } = string.Empty;
    }

    public class IbkrLoginRequest 
    { 
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    
    public class IbkrQrLoginCompleteRequest 
    { 
        public string SessionId { get; set; } = string.Empty;
        public object? SessionCookies { get; set; }
    }
}
