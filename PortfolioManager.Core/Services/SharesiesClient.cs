using System.Net.Http.Json;
using PortfolioManager.Contracts;
using PortfolioManager.Contracts.Models;

namespace PortfolioManager.Core.Services;

public interface ISharesiesClient
{
    Task<SharesiesLoginResponse> LoginAsync(string email, string password, string? mfaCode = null);
    Task<SharesiesProfileResponse?> GetProfileAsync();
    Task<SharesiesPortfolio?> GetPortfolioAsync(string userId, string portfolioId);
    Task<SharesiesInstrumentResponse?> GetInstrumentsAsync(string userId, List<string> instrumentIds);
}

public class SharesiesClient : ISharesiesClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCacheWrapper _memoryCacheWrapper;

    public SharesiesClient(HttpClient httpClient, IMemoryCacheWrapper memoryCacheWrapper)
    {
        _httpClient = httpClient;
        _memoryCacheWrapper = memoryCacheWrapper;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36");
    }

    public async Task<SharesiesLoginResponse> LoginAsync(string email, string password, string? mfaCode = null)
    {
        var loginRequest = new SharesiesLoginRequest
        {
            Email = email,
            Password = password,
            Remember = true
        };

        if (!string.IsNullOrEmpty(mfaCode))
        {
            loginRequest.EmailMfaToken = mfaCode;
        }

        var response = await _httpClient.PostAsJsonAsync($"{Constants.BaseSharesiesApiUrl}/identity/login", loginRequest);

        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<SharesiesLoginResponse>();

            if (loginResponse?.Type == "identity_email_mfa_required" && string.IsNullOrEmpty(mfaCode))
            {
                // MFA required but not provided
                return loginResponse;
            }

            if (loginResponse is { Authenticated: true })
            {
                var userId = loginResponse.User.Id;
                _memoryCacheWrapper.Set(GetRakaiaTokenCacheKey(userId), loginResponse.RakaiaToken);
                _memoryCacheWrapper.Set(GetDistillTokenCacheKey(userId), loginResponse.DistillToken);
                return loginResponse;
            }
        }
        throw new UnauthorizedAccessException($"Sharesies - Unable to login for user: {email}");
    }

    public async Task<SharesiesProfileResponse?> GetProfileAsync()
    {
        return await _httpClient.GetFromJsonAsync<SharesiesProfileResponse>($"{Constants.BaseSharesiesApiUrl}/profiles");
    }

    public async Task<SharesiesPortfolio?> GetPortfolioAsync(string userId, string portfolioId)
    {
        var url = $"{Constants.BasePortfolioSharesiesApiUrl}/portfolios/{portfolioId}/instruments";
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        // Add headers from the provided example
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");

        var rakiaToken = _memoryCacheWrapper.Get<string>(GetRakaiaTokenCacheKey(userId));
        if (!string.IsNullOrEmpty(rakiaToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", rakiaToken);
        }
        
        BindHeaders(request);
        
        var response = await _httpClient.SendAsync(request);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SharesiesPortfolio>();
        }

        return null;
    }

    public async Task<SharesiesInstrumentResponse?> GetInstrumentsAsync(string userId, List<string> instrumentIds)
    {
        const string url = $"{Constants.BaseDataSharesiesApiUrl}/instruments";
        var payload = new
        {
            query = string.Empty,
            instruments = instrumentIds ?? new List<string>(),
            tradingStatuses = new[] { "active", "halt", "closeonly", "notrade", "inactive", "unknown" },
            perPage = 500
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload)
        };

        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");

        var distillToken = _memoryCacheWrapper.Get<string>(GetDistillTokenCacheKey(userId));
        if (!string.IsNullOrEmpty(distillToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", distillToken);
        }

        BindHeaders(request);

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SharesiesInstrumentResponse>();
        }

        return null;
    }

    private static void BindHeaders(HttpRequestMessage request)
    {
        request.Headers.Add("Origin", Constants.Origin);
        request.Headers.Add("Referer", Constants.Origin);
        request.Headers.Add("sec-ch-ua", "\"Google Chrome\";v=\"143\", \"Chromium\";v=\"143\", \"Not A(Brand\";v=\"24\"");
        request.Headers.Add("sec-ch-ua-mobile", "?0");
        request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
        request.Headers.Add("sec-fetch-dest", "empty");
        request.Headers.Add("sec-fetch-mode", "cors");
        request.Headers.Add("sec-fetch-site", "cross-site");
        request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36");
    }
    
    private static string GetDistillTokenCacheKey(string? userId)
    {
        return $"{userId}-_distillToken";
    }

    private static string GetRakaiaTokenCacheKey(string? userId)
    {
        return $"{userId}_rakaiaToken";
    }
}
