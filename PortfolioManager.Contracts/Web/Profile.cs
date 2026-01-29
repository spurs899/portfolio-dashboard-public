using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Web;

public class Profile
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("portfolios")]
    public List<Portfolio>? Portfolios { get; set; }
}