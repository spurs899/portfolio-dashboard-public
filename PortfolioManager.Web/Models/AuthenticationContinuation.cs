namespace PortfolioManager.Web.Models;

public class AuthenticationContinuation
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string? MfaCode { get; set; }
}