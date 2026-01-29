using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Models;

public class IBProfileResponse
{
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}
