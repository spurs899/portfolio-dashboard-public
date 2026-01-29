using Microsoft.JSInterop;
using PortfolioManager.Contracts;

namespace PortfolioManager.Web.Services;

public interface IAuthStateReader
{
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetUserIdAsync();
    Task<string?> GetBrokerageTypeAsync();
    Task<(string? brokerageType, string? userId, Dictionary<string, string>? tokens)> GetAuthDataAsync();
}

public interface IAuthStateWriter
{
    Task SaveAuthStateAsync(string brokerageType, string userId, Dictionary<string, string>? tokens);
    Task ClearAuthStateAsync();
}

// Composite interface for backward compatibility
public interface IAuthStateService : IAuthStateReader, IAuthStateWriter
{
}

public class AuthStateService : IAuthStateService
{
    private const string BrokerageTypeKey = "auth_brokerageType";
    private const string UserIdKey = "auth_userId";
    private const string TokensKey = "auth_tokens";
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
        var brokerageType = await GetFromLocalStorageAsync(BrokerageTypeKey);
        return !string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(brokerageType);
    }

    public async Task<string?> GetUserIdAsync()
    {
        if (_demoMode)
        {
            return "demo-user-123";
        }

        return await GetFromLocalStorageAsync(UserIdKey);
    }

    public async Task<string?> GetBrokerageTypeAsync()
    {
        if (_demoMode)
        {
            return Constants.BrokerageSharesies;
        }

        return await GetFromLocalStorageAsync(BrokerageTypeKey);
    }

    public async Task SaveAuthStateAsync(string brokerageType, string userId, Dictionary<string, string>? tokens)
    {
        if (_demoMode)
        {
            await SetInLocalStorageAsync(DemoAuthKey, "true");
        }
        else
        {
            await SetInLocalStorageAsync(BrokerageTypeKey, brokerageType);
            await SetInLocalStorageAsync(UserIdKey, userId);
            
            if (tokens != null)
            {
                var tokensJson = System.Text.Json.JsonSerializer.Serialize(tokens);
                await SetInLocalStorageAsync(TokensKey, tokensJson);
            }
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
            await RemoveFromLocalStorageAsync(BrokerageTypeKey);
            await RemoveFromLocalStorageAsync(UserIdKey);
            await RemoveFromLocalStorageAsync(TokensKey);
        }
    }

    public async Task<(string? brokerageType, string? userId, Dictionary<string, string>? tokens)> GetAuthDataAsync()
    {
        if (_demoMode)
        {
            return (Constants.BrokerageSharesies, "demo-user-123", new Dictionary<string, string>
            {
                ["RakaiaToken"] = "demo-rakaia-token",
                ["DistillToken"] = "demo-distill-token"
            });
        }

        var brokerageType = await GetFromLocalStorageAsync(BrokerageTypeKey);
        var userId = await GetFromLocalStorageAsync(UserIdKey);
        var tokensJson = await GetFromLocalStorageAsync(TokensKey);
        
        Dictionary<string, string>? tokens = null;
        if (!string.IsNullOrEmpty(tokensJson))
        {
            try
            {
                tokens = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(tokensJson);
            }
            catch
            {
                // Ignore deserialization errors
            }
        }

        return (brokerageType, userId, tokens);
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
