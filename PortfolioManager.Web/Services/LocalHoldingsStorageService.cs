using Blazored.LocalStorage;
using PortfolioManager.Contracts.Models.Shared;
using PortfolioManager.Contracts.Web;

namespace PortfolioManager.Web.Services;

public class LocalHoldingsStorageService : IHoldingsStorageService
{
    private readonly ILocalStorageService _localStorage;
    private const string StorageKey = "portfolio_holdings";

    public LocalHoldingsStorageService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<List<Holding>> GetHoldingsAsync()
    {
        try
        {
            var holdings = await _localStorage.GetItemAsync<List<Holding>>(StorageKey);
            return holdings ?? new List<Holding>();
        }
        catch
        {
            return new List<Holding>();
        }
    }

    public async Task SaveHoldingsAsync(List<Holding> holdings)
    {
        await _localStorage.SetItemAsync(StorageKey, holdings);
    }

    public async Task<bool> AddHoldingAsync(Holding holding)
    {
        var holdings = await GetHoldingsAsync();
        
        // Check if holding with same symbol and brokerage already exists
        var existing = holdings.FirstOrDefault(h => 
            h.Symbol.Equals(holding.Symbol, StringComparison.OrdinalIgnoreCase) && 
            h.BrokerageType.Equals(holding.BrokerageType, StringComparison.OrdinalIgnoreCase));
        
        if (existing != null)
        {
            // Merge with existing holding: add shares and calculate weighted average cost
            var totalShares = existing.Shares + holding.Shares;
            var weightedAvgCost = totalShares > 0 
                ? ((existing.Shares * existing.AverageCost) + (holding.Shares * holding.AverageCost)) / totalShares
                : 0;
            
            existing.Shares = totalShares;
            existing.AverageCost = weightedAvgCost;
            existing.AddedDate = DateTime.UtcNow; // Update to current time
            
            // Keep other fields from new holding if they've changed
            if (!string.IsNullOrEmpty(holding.Name))
            {
                existing.Name = holding.Name;
            }
            if (!string.IsNullOrEmpty(holding.Notes))
            {
                existing.Notes = holding.Notes;
            }
            
            await SaveHoldingsAsync(holdings);
            return true; // Indicates a merge occurred
        }
        else
        {
            holdings.Add(holding);
            await SaveHoldingsAsync(holdings);
            return false; // Indicates a new holding was added
        }
    }

    public async Task UpdateHoldingAsync(string symbol, Holding updatedHolding)
    {
        var holdings = await GetHoldingsAsync();
        var index = holdings.FindIndex(h => 
            h.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase) &&
            h.BrokerageType.Equals(updatedHolding.BrokerageType, StringComparison.OrdinalIgnoreCase));
        
        if (index >= 0)
        {
            holdings[index] = updatedHolding;
            await SaveHoldingsAsync(holdings);
        }
        else
        {
            throw new InvalidOperationException($"Holding {symbol} not found");
        }
    }

    public async Task DeleteHoldingAsync(string symbol)
    {
        var holdings = await GetHoldingsAsync();
        holdings.RemoveAll(h => h.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        await SaveHoldingsAsync(holdings);
    }

    public async Task DeleteHoldingAsync(string symbol, string brokerageType)
    {
        var holdings = await GetHoldingsAsync();
        holdings.RemoveAll(h => 
            h.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase) && 
            h.BrokerageType.Equals(brokerageType, StringComparison.OrdinalIgnoreCase));
        await SaveHoldingsAsync(holdings);
    }

    public async Task ClearHoldingsAsync()
    {
        await _localStorage.RemoveItemAsync(StorageKey);
    }
}
