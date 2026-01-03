using PortfolioManager.Contracts.Models;
using PortfolioManager.Core.Interfaces;

namespace PortfolioManager.Api.Services;

public class SharesiesClient : ISharesiesClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://app.sharesies.com/api";
    private string? _rakaiaToken;
    private string? _distillToken;

    public SharesiesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        //_httpClient.BaseAddress = new Uri(BaseUrl);
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

        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/identity/login", loginRequest);
        
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
        return await _httpClient.GetFromJsonAsync<SharesiesProfileResponse>($"{BaseUrl}/profiles");
    }

    public async Task<SharesiesPortfolio?> GetPortfolioAsync(string? portfolioId = null)
    {
        //https://portfolio.sharesies.nz/api/v1/portfolios/56a69fe9-3473-4734-9e3f-ece717fa10fe/instruments
        var url = $"https://portfolio.sharesies.nz/api/v1/portfolios/{portfolioId}/instruments";
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        // Add headers from the provided example
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
        
        if (!string.IsNullOrEmpty(_rakaiaToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _rakaiaToken);
        }

        request.Headers.Add("Origin", "https://app.sharesies.com");
        request.Headers.Add("Referer", "https://app.sharesies.com/");
        request.Headers.Add("sec-ch-ua", "\"Google Chrome\";v=\"143\", \"Chromium\";v=\"143\", \"Not A(Brand\";v=\"24\"");
        request.Headers.Add("sec-ch-ua-mobile", "?0");
        request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
        request.Headers.Add("sec-fetch-dest", "empty");
        request.Headers.Add("sec-fetch-mode", "cors");
        request.Headers.Add("sec-fetch-site", "cross-site");
        request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36");
        
        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SharesiesPortfolio>();
        }

        return null;
    }

    public async Task<SharesiesInstrumentResponse?> GetInstrumentsAsync()
    {
        const string url = "https://data.sharesies.nz/api/v1/instruments/info";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        // Add headers from the provided example
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
        
        if (!string.IsNullOrEmpty(_distillToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _distillToken);
        }

        request.Headers.Add("Origin", "https://app.sharesies.com");
        request.Headers.Add("Referer", "https://app.sharesies.com/");
        request.Headers.Add("sec-ch-ua", "\"Google Chrome\";v=\"143\", \"Chromium\";v=\"143\", \"Not A(Brand\";v=\"24\"");
        request.Headers.Add("sec-ch-ua-mobile", "?0");
        request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
        request.Headers.Add("sec-fetch-dest", "empty");
        request.Headers.Add("sec-fetch-mode", "cors");
        request.Headers.Add("sec-fetch-site", "cross-site");
        request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36");

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SharesiesInstrumentResponse>();
        }

        return null;
    }
}
