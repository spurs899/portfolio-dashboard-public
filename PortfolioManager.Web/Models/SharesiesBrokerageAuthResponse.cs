namespace PortfolioManager.Web.Models;

public class SharesiesBrokerageAuthResponse
{
    public bool Success { get; set; }
    public bool Authenticated { get; set; }
    public string? Step { get; set; }
    public string? SessionId { get; set; }
    public string? UserId { get; set; }
    public Dictionary<string, string>? Tokens { get; set; }
    public string? Message { get; set; }
    public bool RequiresMfa { get; set; }
    public string? MfaType { get; set; }
    public bool RequiresQrScan { get; set; }
    public string? QrCodeBase64 { get; set; }
}