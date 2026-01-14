using PortfolioManager.Contracts.Models.Brokerage;
using PortfolioManager.Contracts.Models.Shared;

namespace PortfolioManager.Core.Services.Brokerage;

public class InteractiveBrokersBrokerageService : IBrokerageService
{
    private readonly IInteractiveBrokersClient _ibkrClient;
    private readonly IQrAuthenticationService _qrAuthService;

    public InteractiveBrokersBrokerageService(
        IInteractiveBrokersClient ibkrClient,
        IQrAuthenticationService qrAuthService)
    {
        _ibkrClient = ibkrClient;
        _qrAuthService = qrAuthService;
    }

    public BrokerageType BrokerageType => BrokerageType.InteractiveBrokers;

    public async Task<AuthenticationResult> AuthenticateAsync(AuthenticationCredentials credentials)
    {
        try
        {
            // Step 1: Generate QR code
            var qrResult = await _qrAuthService.GenerateQrCodeAsync(
                credentials.Username,
                credentials.Password);

            return new AuthenticationResult
            {
                IsAuthenticated = false,
                Step = AuthenticationStep.QrCodeGenerated,
                SessionId = qrResult.SessionId,
                QrCodeImage = qrResult.QrImage,
                Metadata = new Dictionary<string, object>
                {
                    ["Message"] = "Scan the QR code with your IBKR mobile app to authenticate"
                }
            };
        }
        catch (Exception ex)
        {
            return new AuthenticationResult
            {
                IsAuthenticated = false,
                Step = AuthenticationStep.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AuthenticationResult> ContinueAuthenticationAsync(AuthenticationCredentials credentials)
    {
        if (string.IsNullOrEmpty(credentials.SessionId))
        {
            return new AuthenticationResult
            {
                IsAuthenticated = false,
                Step = AuthenticationStep.Failed,
                ErrorMessage = "SessionId is required for IBKR authentication continuation"
            };
        }

        try
        {
            // Check authentication status
            var qrResult = await _qrAuthService.CheckAuthenticationStatusAsync(credentials.SessionId);

            if (qrResult.IsAuthenticated)
            {
                // Extract tokens/cookies from session data
                var tokens = new Dictionary<string, string>();
                // TODO: Parse cookies from qrResult.SessionData

                return new AuthenticationResult
                {
                    IsAuthenticated = true,
                    Step = AuthenticationStep.Completed,
                    SessionId = credentials.SessionId,
                    Tokens = tokens
                };
            }

            return new AuthenticationResult
            {
                IsAuthenticated = false,
                Step = AuthenticationStep.AwaitingConfirmation,
                SessionId = credentials.SessionId,
                Metadata = new Dictionary<string, object>
                {
                    ["Message"] = "Waiting for QR code scan confirmation"
                }
            };
        }
        catch (Exception ex)
        {
            return new AuthenticationResult
            {
                IsAuthenticated = false,
                Step = AuthenticationStep.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> ValidateSessionAsync(string sessionId)
    {
        try
        {
            return await _ibkrClient.ValidateSessionAsync();
        }
        catch
        {
            return false;
        }
    }

    public async Task<PortfolioData?> GetPortfolioDataAsync(AuthenticationResult authResult)
    {
        if (!authResult.IsAuthenticated)
            return null;

        try
        {
            var accountResponse = await _ibkrClient.GetAccountAsync();
            if (accountResponse == null)
                return null;

            var accountId = accountResponse.UserId ?? "Unknown";
            
            var positionsResponse = await _ibkrClient.GetPositionsAsync(accountId);
            if (positionsResponse == null)
                return null;

            var userProfile = new UserProfile
            {
                Id = accountId,
                Name = accountResponse.Name ?? accountId,
                Image = string.Empty,
                BrokerageType = BrokerageType.InteractiveBrokers
            };

            var instruments = new List<PortfolioInstrument>();

            return new PortfolioData
            {
                UserProfile = userProfile,
                Instruments = instruments
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting IBKR portfolio data: {ex.Message}");
            return null;
        }
    }
}
