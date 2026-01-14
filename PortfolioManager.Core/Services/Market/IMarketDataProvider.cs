using PortfolioManager.Contracts.Models.Market;

namespace PortfolioManager.Core.Services.Market;

public interface IMarketDataProvider
{
    Task<PolygonMarketStatusResponse?> GetMarketStatusAsync();
}
