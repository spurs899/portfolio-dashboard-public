using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Web;

public class UserProfileDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("brokerageType")]
    public int BrokerageType { get; set; }
}