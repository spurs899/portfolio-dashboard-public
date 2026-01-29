using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using PortfolioManager.Contracts.Models;
using PortfolioManager.Contracts.Web;

namespace PortfolioManager.Web.Services;

public interface IIbkrService
{
    Task<IbkrAuthResult> AuthenticateAsync(string username, string password, Action<string> onQRCodeReceived, Action<string> onStatusUpdate);
    Task<IbkrAccountsResponse?> GetAccountsAsync();
    Task<IbkrPortfolioSummary?> GetPortfolioSummaryAsync(string accountId);
    Task<List<InstrumentDto>?> GetPositionsAsync(string accountId);
    Task<string?> GetStoredUsernameAsync();
    Task<bool> ValidateSessionAsync();
    Task ClearSessionAsync();
}

public class IbkrService : IIbkrService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly string _apiBaseUrl;
    private HubConnection? _hubConnection;
    private const string StorageKey = "ibkr_username";

    public IbkrService(HttpClient httpClient, ILocalStorageService localStorage, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _apiBaseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "https://localhost:7169";
    }

    public async Task<IbkrAuthResult> AuthenticateAsync(
        string username, 
        string password, 
        Action<string> onQRCodeReceived, 
        Action<string> onStatusUpdate)
    {
        try
        {
            // Create SignalR connection
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{_apiBaseUrl}/hubs/ibkrauth")
                .WithAutomaticReconnect()
                .Build();

            // Set up event listeners
            _hubConnection.On<string>("ReceiveQRCode", (base64Image) =>
            {
                onQRCodeReceived?.Invoke(base64Image);
            });

            _hubConnection.On<string>("ReceiveAuthStatus", (message) =>
            {
                onStatusUpdate?.Invoke(message);
            });

            // Connect to SignalR hub
            await _hubConnection.StartAsync();

            // Call authentication endpoint
            var response = await _httpClient.PostAsJsonAsync("/api/ibkrsession/authenticate", new
            {
                username,
                password,
                connectionId = _hubConnection.ConnectionId
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResult>();
                
                if (result?.Success == true)
                {
                    // Store username in browser localStorage for subsequent requests
                    await _localStorage.SetItemAsStringAsync(StorageKey, username);
                }
                
                return new IbkrAuthResult
                {
                    Success = result?.Success ?? false,
                    Message = result?.Message ?? "Authentication completed",
                    Username = username,
                    CookieCount = result?.CookieCount ?? 0
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new IbkrAuthResult
                {
                    Success = false,
                    Message = $"Server error: {response.StatusCode} - {errorContent}"
                };
            }
        }
        catch (Exception ex)
        {
            return new IbkrAuthResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
        finally
        {
            // Clean up SignalR connection
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }
    }

    public async Task<IbkrAccountsResponse?> GetAccountsAsync()
    {
        try
        {
            var username = await GetStoredUsernameAsync();
            if (string.IsNullOrEmpty(username))
            {
                return null; // Not authenticated
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/ibkr/accounts");
            request.Headers.Add("X-IBKR-Username", username);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IbkrAccountsResponse>();
            }
            
            return null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return null;
        }
    }

    public async Task<IbkrPortfolioSummary?> GetPortfolioSummaryAsync(string accountId)
    {
        try
        {
            var username = await GetStoredUsernameAsync();
            if (string.IsNullOrEmpty(username))
            {
                return null;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/ibkr/portfolio/{accountId}");
            request.Headers.Add("X-IBKR-Username", username);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IbkrPortfolioSummary>();
            }
            
            return null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return null;
        }
    }

    public async Task<List<InstrumentDto>?> GetPositionsAsync(string accountId)
    {
        try
        {
            var username = await GetStoredUsernameAsync();
            if (string.IsNullOrEmpty(username))
            {
                return null;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/ibkr/positions/{accountId}");
            request.Headers.Add("X-IBKR-Username", username);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<InstrumentDto>>();
            }
            
            return null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return null;
        }
    }

    public async Task<string?> GetStoredUsernameAsync()
    {
        try
        {
            return await _localStorage.GetItemAsStringAsync(StorageKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ValidateSessionAsync()
    {
        try
        {
            var username = await GetStoredUsernameAsync();
            if (string.IsNullOrEmpty(username))
            {
                return false;
            }

            // Validate session by actually trying to get accounts
            // This will fail if session cookies are expired or invalid
            var accounts = await GetAccountsAsync();
            return accounts?.Accounts != null && accounts.Accounts.Any();
        }
        catch
        {
            return false;
        }
    }

    public async Task ClearSessionAsync()
    {
        try
        {
            await _localStorage.RemoveItemAsync(StorageKey);
        }
        catch
        {
            // Ignore errors
        }
    }
}

// Result classes
public class IbkrAuthResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Username { get; set; }
    public int CookieCount { get; set; }
}

// Private class for deserialization
file class AuthResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int CookieCount { get; set; }
}
