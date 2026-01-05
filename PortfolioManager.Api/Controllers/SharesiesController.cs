using Microsoft.AspNetCore.Mvc;
using PortfolioManager.Core.Services;

namespace PortfolioManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SharesiesController : ControllerBase
    {
        private readonly ISharesiesClient _sharesiesClient;
        public SharesiesController(ISharesiesClient sharesiesClient)
        {
            _sharesiesClient = sharesiesClient;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            var success = await _sharesiesClient.LoginAsync(email, password);
            return success is { Authenticated: true } ? Ok() : Unauthorized();
        }

        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            var profile = await _sharesiesClient.GetProfileAsync();
            return profile != null ? Ok(profile) : NotFound();
        }

        [HttpGet("portfolio")]
        public async Task<IActionResult> Portfolio()
        {
            var portfolio = await _sharesiesClient.GetPortfolioAsync();
            return portfolio != null ? Ok(portfolio) : NotFound();
        }
    }
}
