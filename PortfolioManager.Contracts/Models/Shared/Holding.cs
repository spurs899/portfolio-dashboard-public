namespace PortfolioManager.Contracts.Models.Shared;

public class Holding
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Shares { get; set; }
    public decimal AverageCost { get; set; }
    public string BrokerageType { get; set; } = "Manual"; // Sharesies, IBKR, Manual, etc.
    public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
