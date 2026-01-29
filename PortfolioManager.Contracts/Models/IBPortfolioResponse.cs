using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PortfolioManager.Contracts.Models;

public class IBPortfolioResponse
{
    [JsonPropertyName("positions")]
    public List<IBPosition>? Positions { get; set; }
}

public class IBPosition
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("market_value")]
    public decimal MarketValue { get; set; }
}
