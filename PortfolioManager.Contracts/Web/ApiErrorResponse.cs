using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Web;

public class ApiErrorResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("requiresMfa")]
    public bool RequiresMfa { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}