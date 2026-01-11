using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Web;

public class LoginResponse
{
    [JsonPropertyName("authenticated")]
    public bool Authenticated { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("user")]
    public UserInfo? User { get; set; }
    
    [JsonPropertyName("rakaia_token")]
    public string? RakaiaToken { get; set; }
    
    [JsonPropertyName("distill_token")]
    public string? DistillToken { get; set; }
}