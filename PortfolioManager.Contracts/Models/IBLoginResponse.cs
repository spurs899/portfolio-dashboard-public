using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Models;

public class IBLoginResponse
{
    [JsonPropertyName("auth_res")]
    public string? AuthRes { get; set; }

    [JsonPropertyName("reached_max_login")]
    public bool ReachedMaxLogin { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonIgnore]
    public bool Authenticated => AuthRes == "true";

    [JsonPropertyName("token")]
    public string? Token { get; set; }
}
