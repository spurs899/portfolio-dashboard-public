using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace PortfolioManager.Web.Components;

public partial class DashboardHeader
{
    [Parameter]
    public string Currency { get; set; } = "";

    [Parameter]
    public EventCallback OnRefresh { get; set; }

    private List<BreadcrumbItem> _breadcrumbItems = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Home", href: "/", icon: Icons.Material.Filled.Home),
        new BreadcrumbItem("Dashboard", href: "/", disabled: true)
    };
}
