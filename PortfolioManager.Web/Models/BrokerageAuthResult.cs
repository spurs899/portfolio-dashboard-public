using PortfolioManager.Contracts.Models.Brokerage;

namespace PortfolioManager.Web.Models;

public class BrokerageAuthResult
{
    public bool Success { get; set; }
    public bool Authenticated { get; set; }
    public AuthenticationStep Step { get; set; }
    public string? SessionId { get; set; }
    public string? UserId { get; set; }
    public Dictionary<string, string>? Tokens { get; set; }
    public string? Message { get; set; }
    
    // MFA
    public bool RequiresMfa { get; set; }
    public string? MfaType { get; set; }
    
    // QR Code
    public bool RequiresQrScan { get; set; }
    public string? QrCodeBase64 { get; set; }
}