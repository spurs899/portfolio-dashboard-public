using PortfolioManager.Contracts.Web;

namespace PortfolioManager.Web.Services;

public class MockPriceService : IPriceService
{
    public async Task<Dictionary<string, PriceData>> GetPricesAsync(IEnumerable<string> symbols)
    {
        await Task.Delay(200); // Simulate network delay
        
        var prices = new Dictionary<string, PriceData>();
        foreach (var symbol in symbols)
        {
            prices[symbol] = await GetPriceAsync(symbol) ?? new PriceData();
        }
        return prices;
    }

    public async Task<PriceData?> GetPriceAsync(string symbol)
    {
        await Task.Delay(100); // Simulate network delay
        
        // Use symbol hash as seed for consistent "random" data per symbol
        var random = new Random(symbol.GetHashCode());
        
        // Generate realistic-looking prices based on symbol
        var basePrice = random.Next(10, 500) + (decimal)random.NextDouble();
        var changePercent = (decimal)(random.NextDouble() * 10 - 5); // -5% to +5%
        var dailyChange = basePrice * changePercent / 100;
        var previousClose = basePrice - dailyChange;
        
        return new PriceData
        {
            CurrentPrice = basePrice,
            Change = dailyChange,
            ChangePercent = changePercent,
            PreviousClose = previousClose
        };
    }
}
