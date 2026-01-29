using Microsoft.AspNetCore.Components;

namespace PortfolioManager.Web.Components;

public partial class PortfolioStatsCards
{
    [Parameter]
    public decimal TotalValue { get; set; }

    [Parameter]
    public decimal DailyReturn { get; set; }

    [Parameter]
    public decimal DailyReturnPercentage { get; set; }

    [Parameter]
    public int HoldingsCount { get; set; }
}
