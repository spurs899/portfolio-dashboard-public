namespace PortfolioManager.Contracts.Web;

public class MarketStatusInfo
{
    public bool IsOpen { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string NyseTime { get; set; } = string.Empty;
}