namespace PortfolioManager.Contracts.Web;

public class LoginResult
{
    public bool Success { get; set; }
    public bool RequiresMfa { get; set; }
    public string? UserId { get; set; }
    public string? Message { get; set; }
    public string? RakaiaToken { get; set; }
    public string? DistillToken { get; set; }
}