using System.Net.Http.Json;
using PortfolioManager.Contracts;
using PortfolioManager.Contracts.Models;

namespace PortfolioManager.Core.Services;

public interface ISharesiesClient
{
    Task<SharesiesLoginResponse> LoginAsync(string email, string password);
    Task<SharesiesProfileResponse?> GetProfileAsync();
    Task<SharesiesPortfolio?> GetPortfolioAsync(string? portfolioId = null);
    Task<SharesiesInstrumentResponse?> GetInstrumentsAsync();
}

public class SharesiesClient : ISharesiesClient
{
    private readonly HttpClient _httpClient;
    private string? _rakaiaToken;
    private string? _distillToken;

    public SharesiesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36");
    }

    public async Task<SharesiesLoginResponse> LoginAsync(string email, string password)
    {
        var loginRequest = new SharesiesLoginRequest
        {
            Email = email,
            Password = password,
            Remember = true
        };

        var response = await _httpClient.PostAsJsonAsync($"{Constants.BaseSharesiesApiUrl}/identity/login", loginRequest);
        
        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<SharesiesLoginResponse>();
            // Sharesies uses cookies for subsequent requests, so we don't necessarily need to handle the token manually if HttpClient is configured with a CookieContainer
            
            if (loginResponse is { Authenticated: true })
            {
                _rakaiaToken = loginResponse.RakaiaToken;
                _distillToken = loginResponse.DistillToken;
                return loginResponse;
            }
        }
        return null;
    }

    public async Task<SharesiesProfileResponse?> GetProfileAsync()
    {
        return await _httpClient.GetFromJsonAsync<SharesiesProfileResponse>($"{Constants.BaseSharesiesApiUrl}/profiles");
    }

    public async Task<SharesiesPortfolio?> GetPortfolioAsync(string? portfolioId = null)
    {
        var url = $"{Constants.BasePortfolioSharesiesApiUrl}/portfolios/{portfolioId}/instruments";
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        // Add headers from the provided example
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
        
        if (!string.IsNullOrEmpty(_rakaiaToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _rakaiaToken);
        }

        BindHeaders(request);
        
        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SharesiesPortfolio>();
        }

        return null;
    }

    public async Task<SharesiesInstrumentResponse?> GetInstrumentsAsync()
    {
        const string url = $"{Constants.BaseDataSharesiesApiUrl}/instruments/info";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        // Add headers from the provided example
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
        
        if (!string.IsNullOrEmpty(_distillToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _distillToken);
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
}
