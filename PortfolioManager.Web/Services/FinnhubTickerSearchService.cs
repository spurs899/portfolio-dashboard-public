using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PortfolioManager.Contracts.Models.Shared;
using PortfolioManager.Contracts.Web;

namespace PortfolioManager.Web.Services;

public class FinnhubTickerSearchService : ITickerSearchService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string BaseUrl = "https://finnhub.io/api/v1";

    public FinnhubTickerSearchService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Finnhub:ApiKey"] ?? throw new InvalidOperationException("Finnhub API key not configured");
        
        Console.WriteLine($"[Finnhub] Initialized with API key: {_apiKey.Substring(0, 4)}...");
    }

    public async Task<List<TickerSearchResult>> SearchTickersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 1)
        {
            return new List<TickerSearchResult>();
        }

        try
        {
            Console.WriteLine($"[Finnhub] Searching for: '{query}'");
            var response = await _httpClient.GetAsync($"{BaseUrl}/search?q={Uri.EscapeDataString(query)}&token={_apiKey}");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Finnhub] API error: {response.StatusCode} - {error}");
                return new List<TickerSearchResult>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FinnhubSearchResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Result == null)
            {
                Console.WriteLine("[Finnhub] No results found");
                return new List<TickerSearchResult>();
            }

            var results = result.Result
                .Where(r => !string.IsNullOrWhiteSpace(r.Symbol))
                .Take(10) // Limit to 10 results
                .Select(r => new TickerSearchResult
                {
                    Symbol = r.Symbol ?? string.Empty,
                    Name = r.Description ?? string.Empty,
                    Type = r.Type,
                    Exchange = r.DisplaySymbol?.Contains(':') == true 
                        ? r.DisplaySymbol.Split(':')[0] 
                        : null
                })
                .ToList();
                
            Console.WriteLine($"[Finnhub] Found {results.Count} results");
            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Finnhub] Error searching tickers: {ex.Message}");
            return new List<TickerSearchResult>();
        }
    }

    private class FinnhubSearchResponse
    {
        public int Count { get; set; }
        public List<FinnhubSearchItem>? Result { get; set; }
    }

    private class FinnhubSearchItem
    {
        public string? Description { get; set; }
        public string? DisplaySymbol { get; set; }
        public string? Symbol { get; set; }
        public string? Type { get; set; }
    }
}
