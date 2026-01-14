using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Models.Market;

public class MarketStatusResponse
{
    [JsonPropertyName("market")]
    public string Market { get; set; } = string.Empty;

    [JsonPropertyName("serverTime")]
    public string ServerTime { get; set; } = string.Empty;

    [JsonPropertyName("nyseStatus")]
    public string NyseStatus { get; set; } = string.Empty;

    [JsonPropertyName("nasdaqStatus")]
    public string NasdaqStatus { get; set; } = string.Empty;

    [JsonPropertyName("isOpen")]
    public bool IsOpen { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}
