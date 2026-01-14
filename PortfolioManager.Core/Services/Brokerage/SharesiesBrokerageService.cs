using PortfolioManager.Contracts.Models.Brokerage;
using PortfolioManager.Contracts.Models.Shared;

namespace PortfolioManager.Core.Services.Brokerage;

public class SharesiesBrokerageService : IBrokerageService
{
    private readonly ISharesiesClient _sharesiesClient;

    public SharesiesBrokerageService(ISharesiesClient sharesiesClient)
    {
        _sharesiesClient = sharesiesClient;
    }

    public BrokerageType BrokerageType => BrokerageType.Sharesies;

    public async Task<AuthenticationResult> AuthenticateAsync(AuthenticationCredentials credentials)
    {
        try
        {
            var loginResponse = await _sharesiesClient.LoginAsync(
                credentials.Username,
                credentials.Password,
                credentials.MfaCode);

            if (loginResponse.Type == "identity_email_mfa_required")
            {
                return new AuthenticationResult
                {
                    IsAuthenticated = false,
                    Step = AuthenticationStep.MfaRequired,
                    MfaType = "email",
                    MfaMessage = "Please check your email for the verification code",
                    ErrorMessage = "MFA code required"
                };
            }

            if (loginResponse.Authenticated)
            {
                var tokens = new Dictionary<string, string>();
                
                if (!string.IsNullOrEmpty(loginResponse.RakaiaToken))
                    tokens["RakaiaToken"] = loginResponse.RakaiaToken;
                
                if (!string.IsNullOrEmpty(loginResponse.DistillToken))
                    tokens["DistillToken"] = loginResponse.DistillToken;

                return new AuthenticationResult
                {
                    IsAuthenticated = true,
                    Step = AuthenticationStep.Completed,
                    UserId = loginResponse.User?.Id,
                    Tokens = tokens,
                    SessionId = Guid.NewGuid().ToString()
                };
            }

            return new AuthenticationResult
            {
                IsAuthenticated = false,
                Step = AuthenticationStep.Failed,
                ErrorMessage = "Invalid credentials"
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
        // For Sharesies, continuation is the same as initial auth with MFA code
        if (string.IsNullOrEmpty(credentials.MfaCode))
        {
            return new AuthenticationResult
            {
                IsAuthenticated = false,
                Step = AuthenticationStep.Failed,
                ErrorMessage = "MFA code is required for continuation"
            };
        }

        return await AuthenticateAsync(credentials);
    }

    public async Task<bool> ValidateSessionAsync(string sessionId)
    {
        try
        {
            var profile = await _sharesiesClient.GetProfileAsync();
            return profile != null;
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
            var profileResponse = await _sharesiesClient.GetProfileAsync();
            if (profileResponse?.Profiles == null || profileResponse.Profiles.Count == 0)
                return null;

            var profile = profileResponse.Profiles[0];
            var portfolioId = profile.Portfolios?.FirstOrDefault(x => x.Product == "INVEST")?.Id;
            if (portfolioId == null)
                return null;

            var rakaiaToken = authResult.Tokens?["RakaiaToken"];
            var distillToken = authResult.Tokens?["DistillToken"];

            if (string.IsNullOrEmpty(rakaiaToken) || string.IsNullOrEmpty(distillToken))
                return null;

            var portfolioResponse = await _sharesiesClient.GetPortfolioAsync(
                authResult.UserId,
                portfolioId,
                rakaiaToken);

            if (portfolioResponse?.InstrumentReturns == null || portfolioResponse.InstrumentReturns.Count == 0)
                return null;

            var instrumentIds = portfolioResponse.InstrumentReturns.Keys.ToList();
            
            var instrumentsResponse = await _sharesiesClient.GetInstrumentsAsync(
                authResult.UserId,
                instrumentIds,
                distillToken);

            if (instrumentsResponse?.Instruments == null || instrumentsResponse.Instruments.Count == 0)
                return null;

            var userProfile = new UserProfile
            {
                Id = profile.Id,
                Name = profile.Name,
                Image = profile.Portfolios[0].Image,
                BrokerageType = BrokerageType.Sharesies
            };

            var instruments = portfolioResponse.InstrumentReturns.Select(x =>
            {
                var matchingInstrument = instrumentsResponse.Instruments.FirstOrDefault(z => z.Id == x.Key);
                if (matchingInstrument == null)
                    throw new InvalidOperationException($"Instrument {x.Key} not found in response");

                if (!decimal.TryParse(matchingInstrument.MarketPrice, out var sharePrice))
                    throw new InvalidOperationException($"Invalid market price '{matchingInstrument.MarketPrice}' for instrument {matchingInstrument.Id}");

                return new PortfolioInstrument
                {
                    BrokerageType = BrokerageType.Sharesies,
                    Id = matchingInstrument.Id,
                    Currency = matchingInstrument.Currency,
                    Name = matchingInstrument.Name,
                    SharesOwned = x.Value.SharesOwned,
                    SharePrice = sharePrice,
                    Symbol = matchingInstrument.Symbol,
                    InvestmentValue = x.Value.InvestmentValue,
                    CostBasis = x.Value.CostBasis,
                    TotalReturn = x.Value.TotalReturn,
                    SimpleReturn = x.Value.SimpleReturn,
                    DividendsReceived = x.Value.DividendsReceived
                };
            }).ToList();

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
