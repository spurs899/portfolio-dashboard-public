using Microsoft.AspNetCore.Mvc;
using PortfolioManager.Core.Coordinators;
using PortfolioManager.Core.Services;

namespace PortfolioManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SharesiesController : ControllerBase
    {
        private readonly ISharesiesCoordinator _sharesiesCoordinator;
        public SharesiesController(ISharesiesCoordinator sharesiesCoordinator)
        {
            _sharesiesCoordinator = sharesiesCoordinator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            var success = await _sharesiesCoordinator.Login(email, password);
            return success is { Authenticated: true } ? Ok(success) : Unauthorized();
        }
        
        [HttpPost("login/mfa")]
        public async Task<IActionResult> LoginMfa([FromForm] string email, [FromForm] string password, [FromForm] string mfaCode)
        {
            var success = await _sharesiesCoordinator.LoginProvideMfaCode(email, password, mfaCode);
            return success is { Authenticated: true } ? Ok(success) : Unauthorized();
        }

        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            var profile = await _sharesiesCoordinator.GetProfile();
            return profile != null ? Ok(profile) : NotFound();
        }

        [HttpGet("portfolio")]
        public async Task<IActionResult> Portfolio(string userId)
        {
            var portfolio = await _sharesiesCoordinator.GetAggregatedProfileAndInstrumentsAsync(userId);
            return portfolio.Item1 != null && portfolio.Item2 != null ? Ok(new
            {
                UserProfile = portfolio.Item1,
                Instruments = portfolio.Item2
            }) : NotFound();
        }
    }
}
