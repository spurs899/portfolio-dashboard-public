using PortfolioManager.Contracts.Models.Shared;
using PortfolioManager.Contracts.Web;

namespace PortfolioManager.Web.Services;

public class MockTickerSearchService : ITickerSearchService
{
    private static readonly List<TickerSearchResult> _mockTickers = new()
    {
        // Tech Giants
        new() { Symbol = "AAPL", Name = "Apple Inc.", Type = "Common Stock", Exchange = "NASDAQ" },
        new() { Symbol = "MSFT", Name = "Microsoft Corporation", Type = "Common Stock", Exchange = "NASDAQ" },
        new() { Symbol = "GOOGL", Name = "Alphabet Inc. Class A", Type = "Common Stock", Exchange = "NASDAQ" },
        new() { Symbol = "GOOG", Name = "Alphabet Inc. Class C", Type = "Common Stock", Exchange = "NASDAQ" },
        new() { Symbol = "AMZN", Name = "Amazon.com, Inc.", Type = "Common Stock", Exchange = "NASDAQ" },
        new() { Symbol = "META", Name = "Meta Platforms, Inc.", Type = "Common Stock", Exchange = "NASDAQ" },
        new() { Symbol = "TSLA", Name = "Tesla, Inc.", Type = "Common Stock", Exchange = "NASDAQ" },
        new() { Symbol = "NVDA", Name = "NVIDIA Corporation", Type = "Common Stock", Exchange = "NASDAQ" },
        new() { Symbol = "NFLX", Name = "Netflix, Inc.", Type = "Common Stock", Exchange = "NASDAQ" },
        
        // Finance
        new() { Symbol = "JPM", Name = "JPMorgan Chase & Co.", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "BAC", Name = "Bank of America Corporation", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "WFC", Name = "Wells Fargo & Company", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "V", Name = "Visa Inc.", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "MA", Name = "Mastercard Incorporated", Type = "Common Stock", Exchange = "NYSE" },
        
        // Retail & Consumer
        new() { Symbol = "WMT", Name = "Walmart Inc.", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "HD", Name = "The Home Depot, Inc.", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "NKE", Name = "NIKE, Inc.", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "SBUX", Name = "Starbucks Corporation", Type = "Common Stock", Exchange = "NASDAQ" },
        new() { Symbol = "MCD", Name = "McDonald's Corporation", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "DIS", Name = "The Walt Disney Company", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "COST", Name = "Costco Wholesale Corporation", Type = "Common Stock", Exchange = "NASDAQ" },
        
        // Healthcare & Pharma
        new() { Symbol = "JNJ", Name = "Johnson & Johnson", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "UNH", Name = "UnitedHealth Group Incorporated", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "PFE", Name = "Pfizer Inc.", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "ABBV", Name = "AbbVie Inc.", Type = "Common Stock", Exchange = "NYSE" },
        
        // Energy
        new() { Symbol = "XOM", Name = "Exxon Mobil Corporation", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "CVX", Name = "Chevron Corporation", Type = "Common Stock", Exchange = "NYSE" },
        
        // Semiconductors
        new() { Symbol = "INTC", Name = "Intel Corporation", Type = "Common Stock", Exchange = "NASDAQ" },
        new() { Symbol = "AMD", Name = "Advanced Micro Devices, Inc.", Type = "Common Stock", Exchange = "NASDAQ" },
        new() { Symbol = "QCOM", Name = "QUALCOMM Incorporated", Type = "Common Stock", Exchange = "NASDAQ" },
        
        // Meme Stocks
        new() { Symbol = "GME", Name = "GameStop Corp.", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "AMC", Name = "AMC Entertainment Holdings, Inc.", Type = "Common Stock", Exchange = "NYSE" },
        
        // Other Popular
        new() { Symbol = "PLTR", Name = "Palantir Technologies Inc.", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "RBLX", Name = "Roblox Corporation", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "COIN", Name = "Coinbase Global, Inc.", Type = "Common Stock", Exchange = "NASDAQ" },
        new() { Symbol = "SHOP", Name = "Shopify Inc.", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "SQ", Name = "Block, Inc.", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "PYPL", Name = "PayPal Holdings, Inc.", Type = "Common Stock", Exchange = "NASDAQ" },
        new() { Symbol = "UBER", Name = "Uber Technologies, Inc.", Type = "Common Stock", Exchange = "NYSE" },
        new() { Symbol = "ABNB", Name = "Airbnb, Inc.", Type = "Common Stock", Exchange = "NASDAQ" },
        
        // ETFs
        new() { Symbol = "SPY", Name = "SPDR S&P 500 ETF Trust", Type = "ETF", Exchange = "NYSE" },
        new() { Symbol = "QQQ", Name = "Invesco QQQ Trust", Type = "ETF", Exchange = "NASDAQ" },
        new() { Symbol = "VOO", Name = "Vanguard S&P 500 ETF", Type = "ETF", Exchange = "NYSE" },
        new() { Symbol = "VTI", Name = "Vanguard Total Stock Market ETF", Type = "ETF", Exchange = "NYSE" },
    };

    public Task<List<TickerSearchResult>> SearchTickersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 1)
        {
            return Task.FromResult(new List<TickerSearchResult>());
        }

        var searchTerm = query.ToUpperInvariant();
        
        var results = _mockTickers
            .Where(t => 
                t.Symbol.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();

        return Task.FromResult(results);
    }
}
