using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;

namespace PortfolioManager.Web.Services;

public interface IAuthStateService
{
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetUserIdAsync();
    Task SaveAuthStateAsync(string userId, string rakaiaToken, string distillToken);
    Task ClearAuthStateAsync();
    Task<(string? userId, string? rakaiaToken, string? distillToken)> GetAuthDataAsync();
}

public class AuthStateService : IAuthStateService
{
    private const string UserIdKey = "sharesies_userId";
    private const string RakaiaTokenKey = "sharesies_rakaiaToken";
    private const string DistillTokenKey = "sharesies_distillToken";
    private const string DemoAuthKey = "demo_authenticated";
    
    private readonly IJSRuntime _jsRuntime;
    private readonly bool _demoMode;

    public AuthStateService(IJSRuntime jsRuntime, IConfiguration configuration)
    {
        _jsRuntime = jsRuntime;
        _demoMode = configuration.GetValue<bool>("DemoMode");
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        if (_demoMode)
        {
            var demoAuth = await GetFromLocalStorageAsync(DemoAuthKey);
            return demoAuth == "true";
        }

        var userId = await GetFromLocalStorageAsync(UserIdKey);
        var rakaiaToken = await GetFromLocalStorageAsync(RakaiaTokenKey);
        return !string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(rakaiaToken);
    }

    public async Task<string?> GetUserIdAsync()
    {
        if (_demoMode)
        {
            return "demo-user-123";
        }

        return await GetFromLocalStorageAsync(UserIdKey);
    }

    public async Task SaveAuthStateAsync(string userId, string rakaiaToken, string distillToken)
    {
        if (_demoMode)
        {
            await SetInLocalStorageAsync(DemoAuthKey, "true");
        }
        else
        {
            await SetInLocalStorageAsync(UserIdKey, userId);
            await SetInLocalStorageAsync(RakaiaTokenKey, rakaiaToken);
            await SetInLocalStorageAsync(DistillTokenKey, distillToken);
        }
    }

    public async Task ClearAuthStateAsync()
    {
        if (_demoMode)
        {
            await RemoveFromLocalStorageAsync(DemoAuthKey);
        }
        else
        {
            await RemoveFromLocalStorageAsync(UserIdKey);
            await RemoveFromLocalStorageAsync(RakaiaTokenKey);
            await RemoveFromLocalStorageAsync(DistillTokenKey);
        }
    }

    public async Task<(string? userId, string? rakaiaToken, string? distillToken)> GetAuthDataAsync()
    {
        if (_demoMode)
        {
            return ("demo-user-123", "demo-rakaia-token", "demo-distill-token");
        }

        var userId = await GetFromLocalStorageAsync(UserIdKey);
        var rakaiaToken = await GetFromLocalStorageAsync(RakaiaTokenKey);
        var distillToken = await GetFromLocalStorageAsync(DistillTokenKey);
        return (userId, rakaiaToken, distillToken);
    }

    private async Task<string?> GetFromLocalStorageAsync(string key)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch
        {
            return null;
        }
    }

    private async Task SetInLocalStorageAsync(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    private async Task RemoveFromLocalStorageAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
