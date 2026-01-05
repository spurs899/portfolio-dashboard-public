using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioManager.Api.Controllers;

[ApiController]
[Route("api/ibkr")]
public class IbkrController : ControllerBase
{
    // In-memory store for demo; use a distributed cache for production
    private static ConcurrentDictionary<string, (byte[] QrImage, bool Authenticated, object SessionCookies)> _sessions = new();

    [HttpPost("start")] // POST api/ibkr-qr-login/start
    public async Task<IActionResult> StartLogin([FromBody] IbkrLoginRequest req)
    {
        // Launch Playwright automation for IBKR QR login
        var automation = new Services.IbkrQrLoginAutomation();
        var result = await automation.StartLoginAsync(req.Username, req.Password);
        _sessions[result.SessionId] = (result.QrImage, result.Authenticated, result.SessionCookies);
        return Ok(new { sessionId = result.SessionId, qrImageBase64 = Convert.ToBase64String(result.QrImage) });
    }

    [HttpGet("status/{sessionId}")]
    public IActionResult GetStatus(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var data))
        {
            return Ok(new { authenticated = data.Authenticated });
        }
        return NotFound();
    }

    // This would be called by the Playwright/Selenium automation after login is complete
    [HttpPost("complete")] // POST api/ibkr-qr-login/complete
    public IActionResult Complete([FromBody] IbkrQrLoginCompleteRequest req)
    {
        if (_sessions.TryGetValue(req.SessionId, out var data))
        {
            _sessions[req.SessionId] = (data.QrImage, true, req.SessionCookies);
            return Ok();
        }
        return NotFound();
    }

    public class IbkrLoginRequest { public string Username { get; set; } public string Password { get; set; } }
    public class IbkrQrLoginCompleteRequest { public string SessionId { get; set; } public object SessionCookies { get; set; } }
}
