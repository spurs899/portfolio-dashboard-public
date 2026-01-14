using PortfolioManager.Contracts.Models.Market;

namespace PortfolioManager.Core.Services.Market;

public interface IMarketStatusCalculator
{
    MarketStatusResponse CalculateMarketStatus();
}
