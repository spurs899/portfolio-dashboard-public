using PortfolioManager.Contracts.Models;
using PortfolioManager.Contracts.Models.Shared;
using PortfolioManager.Core.Services;

namespace PortfolioManager.Core.Coordinators;

public interface ISharesiesCoordinator
{
    Task<SharesiesLoginResponse> Login(string email, string password);
    Task<SharesiesLoginResponse> LoginProvideMfaCode(string email, string password, string mfaCode);
    Task<SharesiesProfileResponse?> GetProfile();
    Task<(UserProfile?, List<PortfolioInstrument>?)> GetAggregatedProfileAndInstrumentsAsync(string userId, string rakaiaToken, string distillToken);
}

public class SharesiesCoordinator : ISharesiesCoordinator
{
    private readonly ISharesiesClient _sharesiesClient;

    public SharesiesCoordinator(ISharesiesClient sharesiesClient)
    {
        _sharesiesClient = sharesiesClient;
    }

    public async Task<SharesiesLoginResponse> Login(string email, string password)
    {
        return await _sharesiesClient.LoginAsync(email, password);
    }
    
    public async Task<SharesiesLoginResponse> LoginProvideMfaCode(string email, string password, string mfaCode)
    {
        return await _sharesiesClient.LoginAsync(email, password, mfaCode);
    }
    
    public async Task<SharesiesProfileResponse?> GetProfile()
    {
        return await _sharesiesClient.GetProfileAsync();
    }
    
    public async Task<(UserProfile?, List<PortfolioInstrument>?)> GetAggregatedProfileAndInstrumentsAsync(string userId, string rakaiaToken, string distillToken)
    {
        try
        {
            var profileResponse = await _sharesiesClient.GetProfileAsync();
            if (profileResponse?.Profiles == null || profileResponse.Profiles.Count == 0)
                return (null, null);

            var profile = profileResponse.Profiles[0];
            var portfolioId = profile.Portfolios?.FirstOrDefault(x => x.Product == "INVEST")?.Id;
            if (portfolioId == null)
                return (null, null);

            var portfolioResponse = await _sharesiesClient.GetPortfolioAsync(userId, portfolioId, rakaiaToken);
            if (portfolioResponse?.InstrumentReturns == null || portfolioResponse.InstrumentReturns.Count == 0)
                return (null, null);

            var instrumentIds = portfolioResponse.InstrumentReturns.Keys.ToList();
            
            var instrumentsResponse = await _sharesiesClient.GetInstrumentsAsync(userId, instrumentIds, distillToken);
            if (instrumentsResponse?.Instruments == null || instrumentsResponse.Instruments.Count == 0)
                return (null, null);

            var userProfile = new UserProfile
            {
                Id = profile.Id,
                Name = profile.Name,
                Image = profile.Portfolios[0].Image,
                BrokerageType = BrokerageType.Sharesies
            };

            var mappedInstruments = portfolioResponse.InstrumentReturns.Select(x =>
            {
                var matchingInstrument = instrumentsResponse.Instruments.FirstOrDefault(z => z.Id == x.Key);
                if (matchingInstrument == null)
                    throw new InvalidOperationException($"Instrument {x.Key} not found in response");

                if (!decimal.TryParse(matchingInstrument.MarketPrice, out var sharePrice))
                    throw new InvalidOperationException($"Invalid market price '{matchingInstrument.MarketPrice}' for instrument {matchingInstrument.Id}");

                var portfolioInstrument = new PortfolioInstrument
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
                return portfolioInstrument;
            }).ToList();
            
            return (userProfile, mappedInstruments);
        }
        catch (Exception ex)
        {
            // Log the exception and return null to indicate failure
            // In production, consider using ILogger
            Console.WriteLine($"Error getting aggregated profile and instruments: {ex.Message}");
            return (null, null);
        }
    }

}