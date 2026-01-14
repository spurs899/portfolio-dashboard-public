namespace PortfolioManager.Contracts.Models.Brokerage;

public enum AuthenticationStep
{
    InitialCredentials,
    MfaRequired,
    QrCodeGenerated,
    AwaitingConfirmation,
    Completed,
    Failed
}

public class AuthenticationResult
{
    public bool IsAuthenticated { get; set; }
    public AuthenticationStep Step { get; set; }
    public string? SessionId { get; set; }
    public string? UserId { get; set; }
    public Dictionary<string, string>? Tokens { get; set; }
    public string? ErrorMessage { get; set; }
    
    // MFA-specific
    public string? MfaType { get; set; }
    public string? MfaMessage { get; set; }
    
    // QR Code-specific
    public byte[]? QrCodeImage { get; set; }
    public string? QrCodeImageBase64 => QrCodeImage != null ? Convert.ToBase64String(QrCodeImage) : null;
    
    // Additional metadata
    public Dictionary<string, object>? Metadata { get; set; }
}
