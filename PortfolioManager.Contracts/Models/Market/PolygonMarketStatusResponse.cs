using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Models.Market;

public class PolygonMarketStatusResponse
{
    [JsonPropertyName("market")]
    public string Market { get; set; } = string.Empty;

    [JsonPropertyName("serverTime")]
    public string ServerTime { get; set; } = string.Empty;

    [JsonPropertyName("exchanges")]
    public PolygonExchanges? Exchanges { get; set; }
}

public class PolygonExchanges
{
    [JsonPropertyName("nyse")]
    public string Nyse { get; set; } = string.Empty;

    [JsonPropertyName("nasdaq")]
    public string Nasdaq { get; set; } = string.Empty;

    [JsonPropertyName("otc")]
    public string? Otc { get; set; }
}
