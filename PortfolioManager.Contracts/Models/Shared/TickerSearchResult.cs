namespace PortfolioManager.Contracts.Models.Shared;

public class TickerSearchResult
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; } // Stock, ETF, etc.
    public string? Exchange { get; set; }
    
    public string DisplayText => $"{Symbol} - {Name}";
}
