using PortfolioManager.Contracts.Models.Shared;

namespace PortfolioManager.Contracts.Web;

public interface ITickerSearchService
{
    Task<List<TickerSearchResult>> SearchTickersAsync(string query);
}
