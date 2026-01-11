using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Web;

public class Portfolio
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("balance")]
    public string? Balance { get; set; }

    [JsonPropertyName("holding_balance")]
    public string? HoldingBalance { get; set; }
}