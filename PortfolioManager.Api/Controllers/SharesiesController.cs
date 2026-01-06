using Microsoft.AspNetCore.Mvc;
using PortfolioManager.Core.Coordinators;

namespace PortfolioManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SharesiesController : ControllerBase
    {
        private readonly ISharesiesCoordinator _sharesiesCoordinator;
        private readonly ILogger<SharesiesController> _logger;

        public SharesiesController(ISharesiesCoordinator sharesiesCoordinator, ILogger<SharesiesController> logger)
        {
            _sharesiesCoordinator = sharesiesCoordinator;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            var success = await _sharesiesCoordinator.Login(email, password);
            _logger.LogInformation($"Successfully logged in {success}");
            return success is { Authenticated: true } ? Ok(success) : Unauthorized();
        }
        
        [HttpPost("login/mfa")]
        public async Task<IActionResult> LoginMfa([FromForm] string email, [FromForm] string password, [FromForm] string mfaCode)
        {
            var success = await _sharesiesCoordinator.LoginProvideMfaCode(email, password, mfaCode);
            _logger.LogInformation($"Successfully logged in with MFA code {mfaCode} - {success}");
            return success is { Authenticated: true } ? Ok(success) : Unauthorized();
        }

        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            var profile = await _sharesiesCoordinator.GetProfile();
            _logger.LogInformation($"Successfully retrieved profile {profile}");
            return profile != null ? Ok(profile) : NotFound();
        }

        [HttpGet("portfolio")]
        public async Task<IActionResult> Portfolio(string userId)
        {
            var portfolio = await _sharesiesCoordinator.GetAggregatedProfileAndInstrumentsAsync(userId);
            
            _logger.LogInformation($"Successfully retrieved portfolio {portfolio}");
            
            return portfolio.Item1 != null && portfolio.Item2 != null ? Ok(new
            {
                UserProfile = portfolio.Item1,
                Instruments = portfolio.Item2
            }) : NotFound();
        }
    }
}
