using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Models;

public class IBAuthenticatorResponse
{
    [JsonPropertyName("B")]
    public string? B { get; set; }
    [JsonPropertyName("s")]
    public string? S { get; set; }
    [JsonPropertyName("lp")]
    public bool Lp { get; set; }
    [JsonPropertyName("paper")]
    public string? Paper { get; set; }
    [JsonPropertyName("g")]
    public string? G { get; set; }
    [JsonPropertyName("proto")]
    public string? Proto { get; set; }
    [JsonPropertyName("rsapub")]
    public string? RsaPub { get; set; }
    [JsonPropertyName("user")]
    public string? User { get; set; }
    [JsonPropertyName("hash")]
    public string? Hash { get; set; }
    [JsonPropertyName("N")]
    public string? N { get; set; }
    [JsonPropertyName("info")]
    public string? Info { get; set; }
}
