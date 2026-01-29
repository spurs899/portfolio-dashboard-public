using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Models;

public class SharesiesLoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
    
    [JsonPropertyName("email_mfa_token")]
    public string EmailMfaToken { get; set; } = string.Empty;
    
    [JsonPropertyName("mfa_token")]
    public string MfaToken { get; set; } = string.Empty;
    
    [JsonPropertyName("remember")]
    public bool Remember { get; set; } = true;
}