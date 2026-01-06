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
            var loginResult = await _sharesiesCoordinator.Login(email, password);
            _logger.LogInformation($"Login attempt for {email}. Result type: {loginResult?.Type}, Authenticated: {loginResult?.Authenticated}");
            
            // Check if MFA is required
            if (loginResult is { Type: "identity_email_mfa_required" })
            {
                _logger.LogInformation($"MFA required for {email}");
                return Unauthorized(new 
                { 
                    message = "MFA required. Please check your email for the verification code, then call the /api/Sharesies/login/mfa endpoint with the code.",
                    requiresMfa = true,
                    type = loginResult.Type
                });
            }
            
            if (loginResult is { Authenticated: true })
            {
                _logger.LogInformation($"Successfully logged in {email}");
                return Ok(loginResult);
            }
            
            _logger.LogWarning($"Login failed for {email}");
            return Unauthorized(new { message = "Invalid credentials", requiresMfa = false });
        }
        
        [HttpPost("login/mfa")]
        public async Task<IActionResult> LoginMfa([FromForm] string email, [FromForm] string password, [FromForm] string mfaCode)
        {
            var loginResult = await _sharesiesCoordinator.LoginProvideMfaCode(email, password, mfaCode);
            _logger.LogInformation($"MFA login attempt for {email} with code {mfaCode}. Authenticated: {loginResult?.Authenticated}");
            
            if (loginResult is { Authenticated: true })
            {
                _logger.LogInformation($"Successfully logged in {email} with MFA");
                return Ok(loginResult);
            }
            
            _logger.LogWarning($"MFA login failed for {email}");
            return Unauthorized(new { message = "Invalid MFA code or credentials" });
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
