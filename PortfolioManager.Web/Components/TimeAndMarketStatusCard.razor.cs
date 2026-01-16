using Microsoft.AspNetCore.Components;

namespace PortfolioManager.Web.Components;

public partial class TimeAndMarketStatusCard
{
    [Parameter]
    public string LocalTime { get; set; } = "";

    [Parameter]
    public string LocalTimeZone { get; set; } = "";

    [Parameter]
    public string NyseTime { get; set; } = "";

    [Parameter]
    public string MarketStatus { get; set; } = "";

    private string GetMarketStatusClass()
    {
        return MarketStatus switch
        {
            "Market Open" => "market-status-open",
            "Market Closed (Weekend)" => "market-status-closed",
            "Pre-Market" => "market-status-premarket",
            "After Hours" => "market-status-afterhours",
            _ => "market-status-closed"
        };
    }
}
