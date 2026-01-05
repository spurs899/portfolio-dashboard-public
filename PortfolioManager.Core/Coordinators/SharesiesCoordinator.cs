using PortfolioManager.Contracts.Models;
using PortfolioManager.Contracts.Models.Shared;
using PortfolioManager.Core.Services;

namespace PortfolioManager.Core.Coordinators;

public interface ISharesiesCoordinator
{
    Task<SharesiesLoginResponse> Login(string email, string password);
    Task<SharesiesLoginResponse> LoginProvideMfaCode(string email, string password, string mfaCode);
    Task<(UserProfile, List<PortfolioInstrument>)> GetAggregatedProfileAndInstrumentsAsync();
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
    public async Task<(UserProfile, List<PortfolioInstrument>)> GetAggregatedProfileAndInstrumentsAsync()
    {
        var profileResponse = await _sharesiesClient.GetProfileAsync();
        if (profileResponse == null || profileResponse.Profiles == null || profileResponse.Profiles.Count == 0)
            return (null, null);

        var profile = profileResponse.Profiles[0];
        var portfolioId = profile.Portfolios?.FirstOrDefault(x => x.Product == "INVEST")?.Id;
        if (portfolioId == null)
            return (null, null);

        var portfolioResponse = await _sharesiesClient.GetPortfolioAsync(portfolioId);
        var instrumentIds = portfolioResponse.InstrumentReturns?.Keys.ToList() ?? new List<string>();
        
        var instrumentsResponse = await _sharesiesClient.GetInstrumentsAsync(instrumentIds);

        var userProfile = new UserProfile
        {
            Id = profile.Id,
            Name = profile.Name,
            Image = profile.Portfolios[0].Image,
            BrokerageType = BrokerageType.Sharesies
        };

        var mappedInstruments = portfolioResponse.InstrumentReturns.Select(x =>
        {
            var matchingInstrument = instrumentsResponse.Instruments.First(z => z.Id == x.Key);

            var portfolioInstrument = new PortfolioInstrument
            {
                BrokerageType = BrokerageType.Sharesies,
                Id = matchingInstrument.Id,
                Currency = matchingInstrument.Currency,
                Name = matchingInstrument.Name,
                SharesOwned = x.Value.SharesOwned,
                SharePrice = decimal.Parse(matchingInstrument.MarketPrice),
                Symbol = matchingInstrument.Symbol };
            return portfolioInstrument;
        }).ToList();
        
        return (userProfile, mappedInstruments);
    }

}