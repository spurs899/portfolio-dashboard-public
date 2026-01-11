using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Web;

public class UserInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }
}