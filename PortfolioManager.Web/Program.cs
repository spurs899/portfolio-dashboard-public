using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PortfolioManager.Web;
using PortfolioManager.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Load configuration from appsettings.json
var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var config = await httpClient.GetFromJsonAsync<Dictionary<string, object>>("appsettings.json");
if (config != null)
{
    builder.Configuration.AddInMemoryCollection(config.Select(kvp => 
        new KeyValuePair<string, string?>(kvp.Key, kvp.Value?.ToString())));
}

// Configure HttpClient with API base URL from configuration
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5269";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<ISharesiesService, SharesiesService>();
builder.Services.AddScoped<IAuthStateService, AuthStateService>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
