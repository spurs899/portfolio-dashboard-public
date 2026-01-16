using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PortfolioManager.Api.Helpers;
using PortfolioManager.Api.Models;
using PortfolioManager.Core.Coordinators;

namespace PortfolioManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("auth")]
    public class SharesiesController : ControllerBase
    {
        private const string MfaRequiredType = "identity_email_mfa_required";
        
        private readonly ISharesiesCoordinator _sharesiesCoordinator;
        private readonly ILogger<SharesiesController> _logger;

        public SharesiesController(ISharesiesCoordinator sharesiesCoordinator, ILogger<SharesiesController> logger)
        {
            _sharesiesCoordinator = sharesiesCoordinator;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new 
                { 
                    message = "Invalid request", 
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var loginResult = await _sharesiesCoordinator.Login(request.Email, request.Password);
            
            // Safe logging - hash email instead of plain text
            var emailHash = LoggingHelper.HashEmail(request.Email);
            _logger.LogInformation("Login attempt for user hash: {EmailHash}, Result type: {Type}, Authenticated: {Authenticated}", 
                emailHash, loginResult?.Type, loginResult?.Authenticated);
            
            // Check if MFA is required
            if (loginResult is { Type: MfaRequiredType })
            {
                _logger.LogInformation("MFA required for user hash: {EmailHash}", emailHash);
                return Unauthorized(new 
                { 
                    message = "MFA required. Please check your email for the verification code, then call the /api/Sharesies/login/mfa endpoint with the code.",
                    requiresMfa = true,
                    type = loginResult.Type
                });
            }
            
            if (loginResult is { Authenticated: true })
            {
                _logger.LogInformation("Successfully logged in user hash: {EmailHash}", emailHash);
                return Ok(loginResult);
            }
            
            _logger.LogWarning("Login failed for user hash: {EmailHash}", emailHash);
            return Unauthorized(new { message = "Invalid credentials", requiresMfa = false });
        }
        
        [HttpPost("login/mfa")]
        public async Task<IActionResult> LoginMfa([FromBody] MfaLoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new 
                { 
                    message = "Invalid request", 
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var loginResult = await _sharesiesCoordinator.LoginProvideMfaCode(request.Email, request.Password, request.MfaCode);
            
            // Safe logging - hash email instead of plain text
            var emailHash = LoggingHelper.HashEmail(request.Email);
            _logger.LogInformation("MFA login attempt for user hash: {EmailHash}, Authenticated: {Authenticated}", 
                emailHash, loginResult?.Authenticated);
            
            if (loginResult is { Authenticated: true })
            {
                _logger.LogInformation("Successfully logged in user hash: {EmailHash} with MFA", emailHash);
                return Ok(loginResult);
            }
            
            _logger.LogWarning("MFA login failed for user hash: {EmailHash}", emailHash);
            return Unauthorized(new { message = "Invalid MFA code or credentials" });
        }

        [HttpGet("profile")]
        [EnableRateLimiting("portfolio")]
        public async Task<IActionResult> Profile()
        {
            var profile = await _sharesiesCoordinator.GetProfile();
            _logger.LogInformation("Profile retrieved: {HasProfile}", profile != null);
            return profile != null ? Ok(profile) : NotFound();
        }

        [HttpGet("portfolio")]
        [EnableRateLimiting("portfolio")]
        public async Task<IActionResult> Portfolio(
            [FromQuery] string userId, 
            [FromHeader(Name = "X-Rakaia-Token")] string? rakaiaToken, 
            [FromHeader(Name = "X-Distill-Token")] string? distillToken)
        {
            if (string.IsNullOrWhiteSpace(userId) || userId.Length > 100)
            {
                _logger.LogWarning("Portfolio request with invalid userId");
                return BadRequest(new { message = "Invalid userId" });
            }

            if (string.IsNullOrEmpty(rakaiaToken) || string.IsNullOrEmpty(distillToken))
            {
                _logger.LogWarning("Portfolio request missing authentication tokens");
                return Unauthorized(new { message = "Authentication tokens required" });
            }

            var portfolio = await _sharesiesCoordinator.GetAggregatedProfileAndInstrumentsAsync(userId, rakaiaToken, distillToken);
            
            _logger.LogInformation("Portfolio retrieved: {HasProfile}, {HasInstruments}", 
                portfolio.Item1 != null, portfolio.Item2 != null);
            
            return portfolio.Item1 != null && portfolio.Item2 != null ? Ok(new
            {
                UserProfile = portfolio.Item1,
                Instruments = portfolio.Item2
            }) : NotFound();
        }
    }
}
