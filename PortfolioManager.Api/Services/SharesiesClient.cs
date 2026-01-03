using PortfolioManager.Contracts.Models;
using PortfolioManager.Core.Interfaces;

namespace PortfolioManager.Api.Services;

public class SharesiesClient : ISharesiesClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://app.sharesies.com/api";

    public SharesiesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        //_httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
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
        // Ensure we send a User-Agent that doesn't trigger 403
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        
        var response = await _httpClient.SendAsync(request);
        var readAsStringAsync = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SharesiesPortfolio>();
        }

        return null;
    }
}
