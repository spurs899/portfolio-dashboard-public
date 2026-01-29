using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PortfolioManager.Contracts.Web;

namespace PortfolioManager.Web.Services;

public class FinnhubPriceService : IPriceService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string BaseUrl = "https://finnhub.io/api/v1";

    public FinnhubPriceService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Finnhub:ApiKey"] ?? throw new InvalidOperationException("Finnhub API key not configured");
        
        Console.WriteLine($"[Finnhub] Initialized with API key: {_apiKey.Substring(0, 4)}...");
    }

    public async Task<Dictionary<string, PriceData>> GetPricesAsync(IEnumerable<string> symbols)
    {
        var prices = new Dictionary<string, PriceData>();
        var symbolList = symbols.ToList();
        
        Console.WriteLine($"[FinnhubPrice] Fetching prices for {symbolList.Count} symbols: {string.Join(", ", symbolList)}");

        foreach (var symbol in symbolList)
        {
            var price = await GetPriceAsync(symbol);
            if (price != null)
            {
                prices[symbol] = price;
            }
        }

        Console.WriteLine($"[FinnhubPrice] Successfully fetched {prices.Count} prices");
        return prices;
    }

    public async Task<PriceData?> GetPriceAsync(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/quote?symbol={Uri.EscapeDataString(symbol)}&token={_apiKey}");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[FinnhubPrice] API error for {symbol}: {response.StatusCode} - {error}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var quote = JsonSerializer.Deserialize<FinnhubQuote>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (quote?.C > 0) // 'c' is current price
            {
                var priceData = new PriceData
                {
                    CurrentPrice = quote.C,
                    Change = quote.D,
                    ChangePercent = quote.Dp,
                    PreviousClose = quote.Pc
                };
                
                Console.WriteLine($"[FinnhubPrice] {symbol}: ${priceData.CurrentPrice:F2} ({(priceData.Change >= 0 ? "+" : "")}{priceData.Change:F2} / {(priceData.ChangePercent >= 0 ? "+" : "")}{priceData.ChangePercent:F2}%)");
                return priceData;
            }

            Console.WriteLine($"[FinnhubPrice] No valid price for {symbol}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FinnhubPrice] Error fetching price for {symbol}: {ex.Message}");
            return null;
        }
    }

    private class FinnhubQuote
    {
        public decimal C { get; set; }  // Current price
        public decimal D { get; set; }  // Change
        public decimal Dp { get; set; } // Percent change
        public decimal H { get; set; }  // High price of the day
        public decimal L { get; set; }  // Low price of the day
        public decimal O { get; set; }  // Open price of the day
        public decimal Pc { get; set; } // Previous close price
        public long T { get; set; }     // Timestamp
    }
}
