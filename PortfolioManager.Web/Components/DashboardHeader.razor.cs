using Microsoft.AspNetCore.Components;
using MudBlazor;
using PortfolioManager.Web.Services;

namespace PortfolioManager.Web.Components;

public partial class DashboardHeader
{
    [Parameter]
    public string Currency { get; set; } = "";

    [Parameter]
    public EventCallback OnRefresh { get; set; }
    
    [Inject]
    private ICurrencyService CurrencyService { get; set; } = default!;

    private List<BreadcrumbItem> _breadcrumbItems = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Home", href: "/", icon: Icons.Material.Filled.Home),
        new BreadcrumbItem("Dashboard", href: "/", disabled: true)
    };
    
    private async Task SetCurrency(string currency)
    {
        await CurrencyService.SetCurrencyAsync(currency);
    }
}
