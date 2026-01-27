using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PortfolioManager.Web;
using PortfolioManager.Web.Services;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add localStorage support
builder.Services.AddBlazoredLocalStorage();

// Load configuration from appsettings.json
var isDemoMode = false;
var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var config = await httpClient.GetFromJsonAsync<Dictionary<string, object>>("appsettings.json");
if (config != null)
{
    builder.Configuration.AddInMemoryCollection(config.Select(kvp => 
        new KeyValuePair<string, string?>(kvp.Key, kvp.Value?.ToString())));
    
    if (config.TryGetValue("DemoMode", out var demoModeValue) && demoModeValue is JsonElement element)
    {
        isDemoMode = element.GetBoolean();
    }
}

// Configure HttpClient with API base URL from configuration
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5269";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Brokerage services - New unified service
if (isDemoMode)
{
    builder.Services.AddScoped<ISharesiesService, SharesiesDemoService>();
    builder.Services.AddScoped<IIbkrService, IbkrDemoService>();
}
else
{
    builder.Services.AddScoped<ISharesiesService, SharesiesService>();
    builder.Services.AddScoped<IIbkrService, IbkrService>();
}

// Auth state services
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<IAuthStateService>(sp => sp.GetRequiredService<AuthStateService>());
builder.Services.AddScoped<IAuthStateReader>(sp => sp.GetRequiredService<AuthStateService>());
builder.Services.AddScoped<IAuthStateWriter>(sp => sp.GetRequiredService<AuthStateService>());

// Other common services
builder.Services.AddScoped<IMarketStatusService, MarketStatusService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
