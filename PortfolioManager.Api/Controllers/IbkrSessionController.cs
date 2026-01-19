using Microsoft.AspNetCore.Mvc;
using PortfolioManager.Core.Services;
using System.Net;

namespace PortfolioManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IbkrSessionController : ControllerBase
{
    private readonly IIbkrSessionManager _sessionManager;
    private readonly IIbkrAutomatedAuthService _automatedAuthService;
    private readonly ILogger<IbkrSessionController> _logger;

    public IbkrSessionController(
        IIbkrSessionManager sessionManager,
        IIbkrAutomatedAuthService automatedAuthService,
        ILogger<IbkrSessionController> logger)
    {
        _sessionManager = sessionManager;
        _automatedAuthService = automatedAuthService;
        _logger = logger;
    }

    /// <summary>
    /// 🚀 AUTOMATED: Authenticate with IBKR and capture cookies automatically
    /// Opens browser, handles login + 2FA, captures cookies - all automated!
    /// NOW WITH QR CODE STREAMING via SignalR!
    /// </summary>
    [HttpPost("authenticate")]
    public async Task<IActionResult> AuthenticateAutomated([FromBody] AuthenticateRequest request)
    {
        try
        {
            var result = await _automatedAuthService.AuthenticateAsync(
                request.Username, 
                request.Password,
                request.ConnectionId);

            if (result.Success)
            {
                return Ok(new 
                { 
                    success = true, 
                    message = result.Message,
                    cookieCount = result.CookieCount,
                    username = result.Username
                });
            }

            return BadRequest(new 
            { 
                success = false, 
                message = result.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in automated authentication");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Store IBKR session cookies after user authenticates via web portal
    /// </summary>
    [HttpPost("store-cookies")]
    public IActionResult StoreCookies([FromBody] StoreCookiesRequest request)
    {
        try
        {
            var cookieContainer = new CookieContainer();
            
            foreach (var cookie in request.Cookies)
            {
                cookieContainer.Add(new Cookie(
                    cookie.Name,
                    cookie.Value,
                    cookie.Path ?? "/",
                    cookie.Domain ?? ".interactivebrokers.com.au"
                )
                {
                    Secure = cookie.Secure,
                    HttpOnly = cookie.HttpOnly
                });
            }
            
            // Use username or user identity as userId
            var userId = request.UserId ?? User?.Identity?.Name ?? "anonymous";
            
            _sessionManager.SetSessionCookies(userId, cookieContainer);
            
            _logger.LogInformation("Session cookies stored for user: {UserId}", userId);
            
            return Ok(new { success = true, message = "Session cookies stored successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing session cookies");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Check if user has valid session
    /// </summary>
    [HttpGet("check-session")]
    public IActionResult CheckSession([FromQuery] string? userId = null)
    {
        userId ??= User?.Identity?.Name ?? "anonymous";
        var hasSession = _sessionManager.HasValidSession(userId);
        
        return Ok(new { hasSession, userId });
    }

    /// <summary>
    /// Clear user session
    /// </summary>
    [HttpPost("clear-session")]
    public IActionResult ClearSession([FromQuery] string? userId = null)
    {
        userId ??= User?.Identity?.Name ?? "anonymous";
        _sessionManager.ClearSession(userId);
        
        _logger.LogInformation("Session cleared for user: {UserId}", userId);
        
        return Ok(new { success = true, message = "Session cleared" });
    }
}

public class AuthenticateRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ConnectionId { get; set; }
}

public class StoreCookiesRequest
{
    public string? UserId { get; set; }
    public List<CookieDto> Cookies { get; set; } = new();
}

public class CookieDto
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Path { get; set; }
    public string? Domain { get; set; }
    public bool Secure { get; set; }
    public bool HttpOnly { get; set; }
}
