namespace PortfolioManager.Contracts.Models.Brokerage;

public class AuthenticationCredentials
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    
    // For MFA continuation
    public string? MfaCode { get; set; }
    public string? SessionId { get; set; }
    
    // For flexible additional data
    public Dictionary<string, string>? AdditionalData { get; set; }
}
