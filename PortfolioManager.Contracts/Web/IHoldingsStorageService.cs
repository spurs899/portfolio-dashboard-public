using PortfolioManager.Contracts.Models.Shared;

namespace PortfolioManager.Contracts.Web;

public interface IHoldingsStorageService
{
    Task<List<Holding>> GetHoldingsAsync();
    Task SaveHoldingsAsync(List<Holding> holdings);
    Task<bool> AddHoldingAsync(Holding holding);
    Task UpdateHoldingAsync(string symbol, Holding updatedHolding);
    Task DeleteHoldingAsync(string symbol);
    Task DeleteHoldingAsync(string symbol, string brokerageType);
    Task ClearHoldingsAsync();
}
