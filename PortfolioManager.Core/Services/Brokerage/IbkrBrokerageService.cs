using PortfolioManager.Contracts.Models.Brokerage;
using PortfolioManager.Contracts.Models.Shared;

namespace PortfolioManager.Core.Services.Brokerage;

public class IbkrBrokerageService : IBrokerageService
{
    private readonly IIbkrClient _ibkrClient;
    private const int MaxPollingAttempts = 30; // 30 attempts at ~1 second each = 30 seconds timeout
    private const int PollingDelayMs = 1000; // 1 second between polls

    public IbkrBrokerageService(IIbkrClient ibkrClient)
    {
        _ibkrClient = ibkrClient;
    }

    public BrokerageType BrokerageType => BrokerageType.InteractiveBrokers;

    public async Task<AuthenticationResult> AuthenticateAsync(AuthenticationCredentials credentials)
    {
        try
        {
            // Step 1: Initiate authentication with username and password
            var authResponse = await _ibkrClient.InitializeAuthenticationAsync(
                credentials.Username,
                credentials.Password);

            if (!authResponse.Authenticated && !authResponse.Competing)
            {
                // Authentication initiated, waiting for QR code scan
                return new AuthenticationResult
                {
                    IsAuthenticated = false,
                    Step = AuthenticationStep.QrCodeGenerated,
                    MfaMessage = "Please scan the QR code using your mobile phone to complete authentication",
                    ErrorMessage = authResponse.Message,
                    Metadata = new Dictionary<string, object>
                    {
                        { "RequiresPolling", true }
                    }
                };
            }

            if (authResponse.Authenticated)
            {
                // Validate the SSO session
                var validateResponse = await _ibkrClient.ValidateSsoAsync();
                
                if (validateResponse?.Valid == true)
                {
                    return new AuthenticationResult
                    {
                        IsAuthenticated = true,
                        Step = AuthenticationStep.Completed,
                        UserId = validateResponse.UserId ?? validateResponse.User,
                        SessionId = Guid.NewGuid().ToString()
                    };
                }
            }

            return new AuthenticationResult
            {
                IsAuthenticated = false,
                Step = AuthenticationStep.Failed,
                ErrorMessage = authResponse.Message ?? "Authentication failed"
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
        try
        {
            // Poll the authentication status until authenticated or timeout
            for (int attempt = 0; attempt < MaxPollingAttempts; attempt++)
            {
                var authResponse = await _ibkrClient.PollAuthenticationStatusAsync();

                if (authResponse.Authenticated)
                {
                    // Validate the SSO session
                    var validateResponse = await _ibkrClient.ValidateSsoAsync();
                    
                    if (validateResponse?.Valid == true)
                    {
                        return new AuthenticationResult
                        {
                            IsAuthenticated = true,
                            Step = AuthenticationStep.Completed,
                            UserId = validateResponse.UserId ?? validateResponse.User,
                            SessionId = Guid.NewGuid().ToString()
                        };
                    }
                }

                // Still waiting for QR code confirmation
                if (attempt < MaxPollingAttempts - 1)
                {
                    await Task.Delay(PollingDelayMs);
                }
            }

            // Timeout reached
            return new AuthenticationResult
            {
                IsAuthenticated = false,
                Step = AuthenticationStep.Failed,
                ErrorMessage = "Authentication timeout. QR code was not scanned within the allowed time."
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
            var validateResponse = await _ibkrClient.ValidateSsoAsync();
            return validateResponse?.Valid == true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<PortfolioData?> GetPortfolioDataAsync(AuthenticationResult authResult)
    {
        if (!authResult.IsAuthenticated || authResult.UserId == null)
            return null;

        try
        {
            // Step 1: Get accounts
            var accountsResponse = await _ibkrClient.GetAccountsAsync();
            if (accountsResponse?.Accounts == null || accountsResponse.Accounts.Count == 0)
                return null;

            // Use first account (or selected account if available)
            var account = accountsResponse.Accounts.FirstOrDefault();
            if (account?.AccountId == null)
                return null;

            // Step 2: Get positions for the account
            var positions = await _ibkrClient.GetPositionsAsync(account.AccountId);
            if (positions == null || positions.Count == 0)
                return null;

            // Step 3: Get security definitions for all positions
            var conIds = positions
                .Where(p => p.ConId.HasValue)
                .Select(p => p.ConId!.Value)
                .Distinct()
                .ToList();

            var securityDefinitions = await _ibkrClient.GetSecurityDefinitionsAsync(conIds);
            var secDefDict = securityDefinitions?
                .Where(s => s.ConId.HasValue)
                .ToDictionary(s => s.ConId!.Value, s => s) 
                ?? new Dictionary<int, Contracts.Models.IbkrSecurityDefinition>();

            // Step 4: Map to PortfolioInstrument
            var instruments = positions
                .Where(p => p.ConId.HasValue && p.Position > 0) // Only include long positions
                .Select(position =>
                {
                    secDefDict.TryGetValue(position.ConId!.Value, out var secDef);

                    return new PortfolioInstrument
                    {
                        BrokerageType = BrokerageType.InteractiveBrokers,
                        Id = position.ConId.ToString() ?? string.Empty,
                        Symbol = secDef?.Symbol ?? position.Ticker ?? "UNKNOWN",
                        Name = secDef?.CompanyName ?? position.ContractDesc ?? "Unknown",
                        Currency = position.Currency ?? "USD",
                        SharesOwned = position.Position ?? 0,
                        SharePrice = position.MktPrice ?? 0,
                        InvestmentValue = position.MktValue ?? 0,
                        CostBasis = (position.AvgCost ?? 0) * (position.Position ?? 0),
                        TotalReturn = position.UnrealizedPnl ?? 0,
                        SimpleReturn = position.UnrealizedPnl ?? 0, // IBKR provides unrealized P&L directly
                        DividendsReceived = 0 // Not available in positions endpoint
                    };
                })
                .ToList();

            var userProfile = new UserProfile
            {
                Id = authResult.UserId,
                Name = account.DisplayName ?? account.AccountTitle ?? account.AccountId,
                Image = string.Empty, // IBKR doesn't provide profile images
                BrokerageType = BrokerageType.InteractiveBrokers
            };

            return new PortfolioData
            {
                UserProfile = userProfile,
                Instruments = instruments
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting portfolio data: {ex.Message}");
            return null;
        }
    }
}
