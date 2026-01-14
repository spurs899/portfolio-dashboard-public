using System.Net.Http.Json;
using PortfolioManager.Contracts.Web;

namespace PortfolioManager.Web.Services;

public interface ISharesiesService
{
    Task<LoginResult> LoginAsync(string email, string password);
    Task<LoginResult> LoginMfaAsync(string email, string password, string mfaCode);
    Task<ProfileResponse?> GetProfileAsync();
    Task<PortfolioResponse?> GetPortfolioAsync(string userId, string rakaiaToken, string distillToken);
}

public class SharesiesService : ISharesiesService
{
    private readonly HttpClient _httpClient;

    public SharesiesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var formData = new Dictionary<string, string>
        {
            { "email", email },
            { "password", password }
        };

        var response = await _httpClient.PostAsync("api/Sharesies/login", new FormUrlEncodedContent(formData));
        
        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return new LoginResult
            {
                Success = loginResponse?.Authenticated ?? false,
                RequiresMfa = false,
                UserId = loginResponse?.User?.Id,
                RakaiaToken = loginResponse?.RakaiaToken,
                DistillToken = loginResponse?.DistillToken,
                Message = "Login successful"
            };
        }
        
        // Handle 401 responses which could be MFA required or invalid credentials
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return new LoginResult
            {
                Success = false,
                RequiresMfa = errorResponse?.RequiresMfa ?? false,
                Message = errorResponse?.Message ?? "Login failed"
            };
        }

        return new LoginResult { Success = false, Message = "Login failed" };
    }

    public async Task<LoginResult> LoginMfaAsync(string email, string password, string mfaCode)
    {
        var formData = new Dictionary<string, string>
        {
            { "email", email },
            { "password", password },
            { "mfaCode", mfaCode }
        };

        var response = await _httpClient.PostAsync("api/Sharesies/login/mfa", new FormUrlEncodedContent(formData));
        
        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return new LoginResult
            {
                Success = loginResponse?.Authenticated ?? false,
                UserId = loginResponse?.User?.Id,
                RakaiaToken = loginResponse?.RakaiaToken,
                DistillToken = loginResponse?.DistillToken,
                Message = "Login successful"
            };
        }
        
        // Handle 401 responses
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return new LoginResult
            {
                Success = false,
                Message = errorResponse?.Message ?? "MFA verification failed"
            };
        }

        return new LoginResult { Success = false, Message = "MFA login failed" };
    }

    public async Task<ProfileResponse?> GetProfileAsync()
    {
        return await _httpClient.GetFromJsonAsync<ProfileResponse>("api/Sharesies/profile");
    }

    public async Task<PortfolioResponse?> GetPortfolioAsync(string userId, string rakaiaToken, string distillToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/Sharesies/portfolio?userId={userId}");
            request.Headers.Add("X-Rakaia-Token", rakaiaToken);
            request.Headers.Add("X-Distill-Token", distillToken);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PortfolioResponse>();
            }
            
            return null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return null;
        }
    }
}