using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace PortfolioManager.Api.Controllers;

[ApiController]
[Route("api/ibkr")]
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
            // Launch Playwright automation for IBKR QR login
            var automation = new Services.IbkrQrLoginAutomation();
            var result = await automation.StartLoginAsync(req.Username, req.Password);
            
            var sessionData = new SessionData
            {
                QrImage = result.QrImage,
                Authenticated = result.Authenticated,
                SessionCookies = result.SessionCookies,
                CreatedAt = DateTime.UtcNow
            };
            
            // Store in cache with sliding expiration
            _cache.Set(result.SessionId, sessionData, new MemoryCacheEntryOptions
            {
                SlidingExpiration = SessionExpiration,
                PostEvictionCallbacks =
                {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = (key, value, reason, state) =>
                        {
                            _logger.LogInformation($"Session {key} evicted. Reason: {reason}");
                        }
                    }
                }
            });
            
            return Ok(new { sessionId = result.SessionId, qrImageBase64 = Convert.ToBase64String(result.QrImage) });
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

        if (_cache.TryGetValue<SessionData>(sessionId, out var data))
        {
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

        if (_cache.TryGetValue<SessionData>(req.SessionId, out var data))
        {
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

    private class SessionData
    {
        public byte[] QrImage { get; set; } = Array.Empty<byte>();
        public bool Authenticated { get; set; }
        public object? SessionCookies { get; set; }
        public DateTime CreatedAt { get; set; }
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
