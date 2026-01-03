using System.Net.Http.Json;
using System.Text.Json;
using PortfolioManager.Contracts.Models;
using PortfolioManager.Core.Interfaces;

namespace PortfolioManager.Api.Services;

public class SharesiesClient : ISharesiesClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://app.sharesies.nz/api/";

    public SharesiesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var loginRequest = new SharesiesLoginRequest
        {
            Email = email,
            Password = password,
            RememberMe = true
        };

        var response = await _httpClient.PostAsJsonAsync("identity/login", loginRequest);
        
        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<SharesiesLoginResponse>();
            // Sharesies uses cookies for subsequent requests, so we don't necessarily need to handle the token manually if HttpClient is configured with a CookieContainer
            return loginResponse?.AuthenticatedUser != null;
        }

        return false;
    }

    public async Task<SharesiesProfile?> GetProfileAsync()
    {
        return await _httpClient.GetFromJsonAsync<SharesiesProfile>("identity/profile");
    }

    public async Task<SharesiesPortfolio?> GetPortfolioAsync()
    {
        return await _httpClient.GetFromJsonAsync<SharesiesPortfolio>("portfolio/get");
    }
}
