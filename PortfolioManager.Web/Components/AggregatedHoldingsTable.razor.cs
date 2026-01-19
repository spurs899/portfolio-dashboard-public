using Microsoft.AspNetCore.Components;
using PortfolioManager.Web.Helpers;

namespace PortfolioManager.Web.Components;

public partial class AggregatedHoldingsTable
{
    [Parameter]
    public List<ViewModels.AggregatedHoldingsTable.AggregatedHoldingViewModel> Holdings { get; set; } = new();

    private string GetSymbolAvatarText(string symbol) => BrokerageHelpers.GetSymbolAvatarText(symbol);

    private string GetBrokerageName(int brokerageType) => BrokerageHelpers.GetBrokerageName(brokerageType);

    private string GetBrokerageLogoUrl(int brokerageType) => BrokerageHelpers.GetBrokerageIcon(brokerageType);
}
