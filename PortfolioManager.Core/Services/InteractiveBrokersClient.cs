using PortfolioManager.Contracts.Models;
using System.Net.Http.Json;

namespace PortfolioManager.Core.Services;

public interface IIbkrAuthClient
{
    Task<IBLoginResponse?> AuthenticateAsync(string username, string password);
    Task<bool> ValidateSessionAsync();
}

public interface IIbkrDataClient
{
    Task<IBProfileResponse?> GetAccountAsync();
    Task<IBPortfolioResponse?> GetPositionsAsync(string accountId);
}

// Composite interface for backward compatibility
public interface IInteractiveBrokersClient : IIbkrAuthClient, IIbkrDataClient
{
}

public class InteractiveBrokersClient : IInteractiveBrokersClient
{
    private readonly HttpClient _httpClient;
    private string? _authToken;

    public InteractiveBrokersClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private void AddBrowserHeaders(HttpRequestMessage request, bool isPost = false)
    {
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        request.Headers.Add("sec-ch-ua", "\"Google Chrome\";v=\"143\", \"Chromium\";v=\"143\", \"Not A(Brand\";v=\"24\"");
        request.Headers.Add("sec-ch-ua-mobile", "?0");
        request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
        request.Headers.Add("Referer", "https://ndcdyn.interactivebrokers.com/sso/Login?RL=1&locale=en_US");
        request.Headers.Add("Origin", "https://ndcdyn.interactivebrokers.com");
        if (isPost)
        {
            request.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("sec-fetch-site", "same-origin");
            request.Headers.Add("sec-fetch-mode", "cors");
            request.Headers.Add("sec-fetch-dest", "empty");
        }
        else
        {
            request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            request.Headers.Add("sec-fetch-site", "same-origin");
            request.Headers.Add("sec-fetch-mode", "navigate");
            request.Headers.Add("sec-fetch-dest", "document");
        }
    }

    public async Task<IBLoginResponse?> AuthenticateAsync(string username, string password)
    {
        // 1. Pre-auth: GET login page to establish session and cookies
        var preAuthRequest = new HttpRequestMessage(HttpMethod.Get, "https://ndcdyn.interactivebrokers.com/sso/Login?RL=1&locale=en_US");
        AddBrowserHeaders(preAuthRequest, false);
        var httpResponseMessage = await _httpClient.SendAsync(preAuthRequest);
        string preAuthHtml;
        if (httpResponseMessage.Content.Headers.ContentEncoding.Contains("gzip"))
        {
            using var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            using var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress);
            using var reader = new System.IO.StreamReader(gzip);
            preAuthHtml = await reader.ReadToEndAsync();
        }
        else
        {
            preAuthHtml = await httpResponseMessage.Content.ReadAsStringAsync();
        }
        Console.WriteLine($"IBKR Pre-auth HTML: {preAuthHtml.Substring(0, Math.Min(preAuthHtml.Length, 500))}...");

        // 2. Authenticator POST
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("ACTION", "COMPLETETWOFACT"),
            new KeyValuePair<string, string>("USER", username),
            new KeyValuePair<string, string>("SF", "5.2a"),
            new KeyValuePair<string, string>("PUSH", "true"),
            new KeyValuePair<string, string>("counter", "4"),
            new KeyValuePair<string, string>("RESP_TYPE", "JSON"),
            new KeyValuePair<string, string>("VERSION", "1")
        });
        var request = new HttpRequestMessage(HttpMethod.Post, "https://ndcdyn.interactivebrokers.com/sso/Authenticator")
        {
            Content = content
        };
        AddBrowserHeaders(request, true);
        var response = await _httpClient.SendAsync(request);
        var requestResponse = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode) return null;
        string raw;
        if (response.Content.Headers.ContentEncoding.Contains("gzip"))
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress);
            using var reader = new System.IO.StreamReader(gzip);
            raw = await reader.ReadToEndAsync();
        }
        else
        {
            raw = await response.Content.ReadAsStringAsync();
        }
        Console.WriteLine($"IBKR Authenticator raw response: {raw}");
        var result = System.Text.Json.JsonSerializer.Deserialize<IBLoginResponse>(raw);
        _authToken = result?.Token;
        return result;
    }

    public async Task<bool> ValidateSessionAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://ndcdyn.interactivebrokers.com/portal.proxy/v1/portal/sso/validate");
        if (!string.IsNullOrEmpty(_authToken))
            request.Headers.Add("Cookie", $"XYZAB={_authToken}");
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<IBProfileResponse?> GetAccountAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://ndcdyn.interactivebrokers.com/portal.proxy/v1/portal/portfolio2/accounts");
        if (!string.IsNullOrEmpty(_authToken))
            request.Headers.Add("Cookie", $"XYZAB={_authToken}");
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;
        var accounts = await response.Content.ReadFromJsonAsync<List<IBProfileResponse>>();
        return accounts?.Count > 0 ? accounts[0] : null;
    }

    public async Task<IBPortfolioResponse?> GetPositionsAsync(string accountId)
    {
        var url = $"https://ndcdyn.interactivebrokers.com/portal.proxy/v1/portal/portfolio2/{accountId}/positions?sort=marketValue&direction=d";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrEmpty(_authToken))
            request.Headers.Add("Cookie", $"XYZAB={_authToken}");
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;
        var positions = await response.Content.ReadFromJsonAsync<List<IBPortfolioResponse>>();
        return positions?.Count > 0 ? positions[0] : null;
    }
}

//TODO:
// call https://www.interactivebrokers.com.au/sso/Authenticator to use QR code (re-direct/new tab)
// after qrcode provided https://www.interactivebrokers.com.au/portal.proxy/v1/portal/sso/validate
//Account info: https://www.interactivebrokers.com.au/portal.proxy/v1/portal/portfolio2/accounts
//Position info: https://www.interactivebrokers.com.au/portal.proxy/v1/portal/portfolio2/U5435267/positions
