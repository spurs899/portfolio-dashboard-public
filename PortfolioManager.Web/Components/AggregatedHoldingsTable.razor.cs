using Microsoft.AspNetCore.Components;
using PortfolioManager.Contracts;

namespace PortfolioManager.Web.Components;

public partial class AggregatedHoldingsTable
{
    private const int MinSymbolAvatarLength = 1;
    private const int MaxSymbolAvatarLength = 2;

    [Parameter]
    public List<ViewModels.AggregatedHoldingsTable.AggregatedHoldingViewModel> Holdings { get; set; } = new();

    private string GetSymbolAvatarText(string symbol)
    {
        if (string.IsNullOrEmpty(symbol) || symbol.Length < MinSymbolAvatarLength)
            return "??";
        
        return symbol.Substring(0, Math.Min(MaxSymbolAvatarLength, symbol.Length));
    }

    private string GetBrokerageName(int brokerageType)
    {
        return brokerageType switch
        {
            0 => Constants.BrokerageSharesies,
            1 => Constants.BrokerageIbkrAcronym,
            _ => "Unknown"
        };
    }

    private string GetBrokerageLogoUrl(int brokerageType)
    {
        return brokerageType switch
        {
            0 => "images/sharesies-logo.png",
            1 => "images/ibkr-logo.svg",
            _ => ""
        };
    }
}
