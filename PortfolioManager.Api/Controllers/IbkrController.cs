using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

namespace PortfolioManager.Api.Controllers;

[ApiController]
[Route("api/ibkr")]
[EnableRateLimiting("auth")]
public class IbkrController : ControllerBase
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<IbkrController> _logger;
    private static readonly TimeSpan SessionExpiration = TimeSpan.FromMinutes(5);

    public IbkrController(IMemoryCache cache, ILogger<IbkrController> logger)
    {
        _cache = cache;
        _logger = logger;
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
