using System.Net.Http.Json;
using PortfolioManager.Contracts;
using PortfolioManager.Contracts.Models.Brokerage;
using PortfolioManager.Contracts.Web;
using PortfolioManager.Web.Models;

namespace PortfolioManager.Web.Services;

public interface ISharesiesService
{
    Task<BrokerageAuthResult> AuthenticateAsync(string brokerageType, string username, string password);
    Task<BrokerageAuthResult> ContinueAuthenticationAsync(string brokerageType, AuthenticationContinuation continuation);
    Task<PortfolioResponse?> GetPortfolioAsync(AuthenticationResult authResult);
    Task<List<SupportedBrokerage>> GetSupportedBrokeragesAsync();
}

public class SharesiesService : ISharesiesService
{
    private readonly HttpClient _httpClient;
    private readonly bool _demoMode;

    public SharesiesService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _demoMode = configuration.GetValue<bool>("DemoMode");
    }

    public async Task<BrokerageAuthResult> AuthenticateAsync(string brokerageType, string username, string password)
    {
        try
        {
            var credentials = new
            {
                username,
                password
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"api/brokerage/{brokerageType}/authenticate",
                credentials);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SharesiesBrokerageAuthResponse>();
                
                return new BrokerageAuthResult
                {
                    Success = result?.Success ?? false,
                    Authenticated = result?.Authenticated ?? false,
                    Step = ParseStep(result?.Step),
                    SessionId = result?.SessionId,
                    UserId = result?.UserId,
                    Tokens = result?.Tokens,
                    Message = result?.Message,
                    RequiresMfa = result?.RequiresMfa ?? false,
                    MfaType = result?.MfaType,
                    RequiresQrScan = result?.RequiresQrScan ?? false,
                    QrCodeBase64 = result?.QrCodeBase64
                };
            }

            var errorResult = await response.Content.ReadFromJsonAsync<SharesiesBrokerageAuthResponse>();
            return new BrokerageAuthResult
            {
                Success = false,
                Message = errorResult?.Message ?? "Authentication failed"
            };
        }
        catch (Exception ex)
        {
            return new BrokerageAuthResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<BrokerageAuthResult> ContinueAuthenticationAsync(string brokerageType, AuthenticationContinuation continuation)
    {
        try
        {
            var credentials = new
            {
                continuation.Username,
                continuation.Password,
                continuation.SessionId,
                continuation.MfaCode
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"api/brokerage/{brokerageType}/authenticate/continue",
                credentials);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SharesiesBrokerageAuthResponse>();
                
                return new BrokerageAuthResult
                {
                    Success = result?.Success ?? false,
                    Authenticated = result?.Authenticated ?? false,
                    Step = ParseStep(result?.Step),
                    SessionId = result?.SessionId,
                    UserId = result?.UserId,
                    Tokens = result?.Tokens,
                    Message = result?.Message,
                    RequiresMfa = result?.RequiresMfa ?? false,
                    RequiresQrScan = result?.RequiresQrScan ?? false
                };
            }

            var errorResult = await response.Content.ReadFromJsonAsync<SharesiesBrokerageAuthResponse>();
            return new BrokerageAuthResult
            {
                Success = false,
                Message = errorResult?.Message ?? "Authentication continuation failed"
            };
        }
        catch (Exception ex)
        {
            return new BrokerageAuthResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<PortfolioResponse?> GetPortfolioAsync(AuthenticationResult authResult)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/brokerage/sharesies/portfolio", // For now, hardcoded to sharesies
                authResult);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PortfolioResponse>();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<SupportedBrokerage>> GetSupportedBrokeragesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/brokerage/supported");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<SupportedBrokerage>>() 
                       ?? new List<SupportedBrokerage>();
            }

            return new List<SupportedBrokerage>();
        }
        catch
        {
            return new List<SupportedBrokerage>();
        }
    }

    private static AuthenticationStep ParseStep(string? step)
    {
        if (string.IsNullOrEmpty(step))
            return AuthenticationStep.Failed;

        return Enum.TryParse<AuthenticationStep>(step, out var result) 
            ? result 
            : AuthenticationStep.Failed;
    }
}
