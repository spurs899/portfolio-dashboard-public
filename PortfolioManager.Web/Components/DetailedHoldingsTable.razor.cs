using Microsoft.AspNetCore.Components;
using PortfolioManager.Web.Helpers;

namespace PortfolioManager.Web.Components;

public partial class DetailedHoldingsTable
{
    [Parameter]
    public List<ViewModels.DetailedHoldingsTable.HoldingViewModel> Holdings { get; set; } = new();

    private string GetSymbolAvatarText(string symbol) => BrokerageHelpers.GetSymbolAvatarText(symbol);

    private string GetBrokerageName(int brokerageType) => BrokerageHelpers.GetBrokerageName(brokerageType);

    private string GetBrokerageLogoUrl(int brokerageType) => BrokerageHelpers.GetBrokerageIcon(brokerageType);
}
