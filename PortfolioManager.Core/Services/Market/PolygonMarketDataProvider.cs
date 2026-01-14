using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PortfolioManager.Contracts.Models.Market;

namespace PortfolioManager.Core.Services.Market;

public class PolygonMarketDataProvider : IMarketDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PolygonMarketDataProvider> _logger;

    public PolygonMarketDataProvider(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<PolygonMarketDataProvider> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PolygonMarketStatusResponse?> GetMarketStatusAsync()
    {
        var apiKey = _configuration["Polygon:ApiKey"];
        
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Polygon API key not configured");
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync($"https://api.polygon.io/v1/marketstatus/now?apiKey={apiKey}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Polygon API returned {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<PolygonMarketStatusResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching market status from Polygon.io");
            return null;
        }
    }
}
