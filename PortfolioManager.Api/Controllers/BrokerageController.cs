using Microsoft.AspNetCore.Mvc;
using PortfolioManager.Contracts;
using PortfolioManager.Contracts.Models.Brokerage;
using PortfolioManager.Contracts.Models.Shared;
using PortfolioManager.Core.Services.Brokerage;

namespace PortfolioManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrokerageController : ControllerBase
{
    private readonly IBrokerageServiceFactory _brokerageServiceFactory;
    private readonly ILogger<BrokerageController> _logger;

    public BrokerageController(
        IBrokerageServiceFactory brokerageServiceFactory,
        ILogger<BrokerageController> logger)
    {
        _brokerageServiceFactory = brokerageServiceFactory;
        _logger = logger;
    }

    [HttpPost("{brokerageType}/authenticate")]
    public async Task<IActionResult> Authenticate(
        string brokerageType,
        [FromBody] AuthenticationCredentials credentials)
    {
        if (!Enum.TryParse<BrokerageType>(brokerageType, true, out var type))
        {
            return BadRequest(new { message = $"Invalid brokerage type: {brokerageType}" });
        }

        try
        {
            var service = _brokerageServiceFactory.GetBrokerageService(type);
            var result = await service.AuthenticateAsync(credentials);

            return result.Step switch
            {
                AuthenticationStep.Completed => Ok(new
                {
                    success = true,
                    authenticated = true,
                    sessionId = result.SessionId,
                    userId = result.UserId,
                    tokens = result.Tokens
                }),
                
                AuthenticationStep.MfaRequired => Ok(new
                {
                    success = true,
                    requiresMfa = true,
                    mfaType = result.MfaType,
                    message = result.MfaMessage ?? "MFA required",
                    step = result.Step.ToString()
                }),
                
                AuthenticationStep.QrCodeGenerated => Ok(new
                {
                    success = true,
                    requiresQrScan = true,
                    sessionId = result.SessionId,
                    qrCodeBase64 = result.QrCodeImageBase64,
                    message = result.Metadata?["Message"] ?? "Scan QR code to continue",
                    step = result.Step.ToString()
                }),
                
                AuthenticationStep.Failed => BadRequest(new
                {
                    success = false,
                    message = result.ErrorMessage ?? "Authentication failed",
                    step = result.Step.ToString()
                }),
                
                _ => StatusCode(500, new
                {
                    success = false,
                    message = "Unexpected authentication state",
                    step = result.Step.ToString()
                })
            };
        }
        catch (NotSupportedException ex)
        {
            _logger.LogError(ex, "Unsupported brokerage type: {BrokerageType}", brokerageType);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating with {BrokerageType}", brokerageType);
            return StatusCode(500, new { message = "An error occurred during authentication" });
        }
    }

    [HttpPost("{brokerageType}/authenticate/continue")]
    public async Task<IActionResult> ContinueAuthentication(
        string brokerageType,
        [FromBody] AuthenticationCredentials credentials)
    {
        if (!Enum.TryParse<BrokerageType>(brokerageType, true, out var type))
        {
            return BadRequest(new { message = $"Invalid brokerage type: {brokerageType}" });
        }

        try
        {
            var service = _brokerageServiceFactory.GetBrokerageService(type);
            var result = await service.ContinueAuthenticationAsync(credentials);

            return result.Step switch
            {
                AuthenticationStep.Completed => Ok(new
                {
                    success = true,
                    authenticated = true,
                    sessionId = result.SessionId,
                    userId = result.UserId,
                    tokens = result.Tokens
                }),
                
                AuthenticationStep.AwaitingConfirmation => Ok(new
                {
                    success = true,
                    authenticated = false,
                    message = result.Metadata?["Message"] ?? "Still waiting for confirmation",
                    step = result.Step.ToString()
                }),
                
                AuthenticationStep.Failed => BadRequest(new
                {
                    success = false,
                    message = result.ErrorMessage ?? "Authentication continuation failed",
                    step = result.Step.ToString()
                }),
                
                _ => StatusCode(500, new
                {
                    success = false,
                    message = "Unexpected authentication state",
                    step = result.Step.ToString()
                })
            };
        }
        catch (NotSupportedException ex)
        {
            _logger.LogError(ex, "Unsupported brokerage type: {BrokerageType}", brokerageType);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error continuing authentication with {BrokerageType}", brokerageType);
            return StatusCode(500, new { message = "An error occurred during authentication" });
        }
    }

    [HttpPost("{brokerageType}/portfolio")]
    public async Task<IActionResult> GetPortfolio(
        string brokerageType,
        [FromBody] AuthenticationResult authResult)
    {
        if (!Enum.TryParse<BrokerageType>(brokerageType, true, out var type))
        {
            return BadRequest(new { message = $"Invalid brokerage type: {brokerageType}" });
        }

        try
        {
            var service = _brokerageServiceFactory.GetBrokerageService(type);
            var portfolioData = await service.GetPortfolioDataAsync(authResult);

            if (portfolioData == null)
            {
                _logger.LogWarning("Failed to retrieve portfolio data for {BrokerageType}", type);
                return NotFound(new { message = "Portfolio data not found" });
            }

            _logger.LogInformation("Successfully retrieved portfolio for {BrokerageType}", type);
            return Ok(portfolioData);
        }
        catch (NotSupportedException ex)
        {
            _logger.LogError(ex, "Unsupported brokerage type: {BrokerageType}", brokerageType);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving portfolio from {BrokerageType}", brokerageType);
            return StatusCode(500, new { message = "An error occurred while retrieving portfolio data" });
        }
    }

    [HttpGet("supported")]
    public IActionResult GetSupportedBrokerages()
    {
        try
        {
            var services = _brokerageServiceFactory.GetAllBrokerageServices();
            var supportedBrokerages = services.Select(s => new
            {
                type = s.BrokerageType.ToString(),
                name = GetBrokerageName(s.BrokerageType)
            });

            return Ok(supportedBrokerages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported brokerages");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    private static string GetBrokerageName(BrokerageType type)
    {
        return type switch
        {
            BrokerageType.Sharesies => Constants.BrokerageSharesies,
            BrokerageType.InteractiveBrokers => Constants.BrokerageIbkrName,
            _ => type.ToString()
        };
    }
}
