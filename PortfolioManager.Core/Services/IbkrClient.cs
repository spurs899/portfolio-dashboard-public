using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using PortfolioManager.Contracts.Models;
using PortfolioManager.Core.Extensions;

namespace PortfolioManager.Core.Services;

public interface IIbkrAuthClient
{
    Task<IbkrAuthResponse> InitializeAuthenticationAsync(string username, string password);
    Task<IbkrAuthResponse> PollAuthenticationStatusAsync();
    Task<IbkrSsoValidateResponse?> ValidateSsoAsync();
}

public interface IIbkrDataClient
{
    Task<IbkrAccountsResponse?> GetAccountsAsync();
    Task<IbkrPortfolioSummary?> GetPortfolioSummaryAsync(string accountId);
    Task<List<IbkrPosition>?> GetPositionsAsync(string accountId);
    Task<List<IbkrSecurityDefinition>?> GetSecurityDefinitionsAsync(List<int> conIds);
}

// Composite interface for backward compatibility
public interface IIbkrClient : IIbkrAuthClient, IIbkrDataClient
{
}

public class IbkrClient : IIbkrClient
{
    // Configuration for IBKR API access:
    // 
    // OPTION 1 (Recommended): Client Portal Gateway
    //   - Download from: https://www.interactivebrokers.com/en/trading/ib-api.php
    //   - Run locally, default: https://localhost:5000
    //   - Use: "https://localhost:5000"
    //
    // OPTION 2: Direct Web Portal (Requires browser automation or existing session)
    //   - Use: "https://www.interactivebrokers.com.au"
    //   - Note: May get 403 Forbidden without proper browser session/cookies
    //
    // To change, set via appsettings.json or environment variable
    private readonly string _baseUrl;
    private readonly bool _useWebPortal;
    
    private readonly HttpClient _httpClient;
    private readonly ILogger<IbkrClient> _logger;

    public IbkrClient(HttpClient httpClient, ILogger<IbkrClient> logger, string? baseUrl = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Default to local Client Portal Gateway
        _baseUrl = baseUrl ?? "https://localhost:5000";
        _useWebPortal = _baseUrl.Contains("interactivebrokers.com");
        
        _httpClient.BaseAddress = new Uri(_baseUrl);
        
        _logger.LogInformation("IBKR Client initialized with base URL: {BaseUrl} (Web Portal: {UseWebPortal})", 
            _baseUrl, _useWebPortal);
    }
    
    // Path helpers that adjust based on gateway vs web portal
    private string SsoAuthPath => _useWebPortal ? "/sso/Authenticator" : "/sso/Authenticator";
    private string SsoValidatePath => _useWebPortal ? "/portal.proxy/v1/portal/sso/validate" : "/v1/portal/sso/validate";
    private string AccountsPath => _useWebPortal ? "/portal.proxy/v1/portal/portfolio2/accounts" : "/v1/api/portfolio/accounts";
    private string PortfolioSummaryPath => _useWebPortal ? "/portal.proxy/v1/portal/portfolio2/{0}/summary" : "/v1/api/portfolio/{0}/summary";
    private string PositionsPath => _useWebPortal ? "/portal.proxy/v1/portal/portfolio2/{0}/positions" : "/v1/api/portfolio/{0}/positions/0";
    private string SecDefPath => _useWebPortal ? "/portal.proxy/v1/portal/trsrv/secdef" : "/v1/api/trsrv/secdef";

    public async Task<IbkrAuthResponse> InitializeAuthenticationAsync(string username, string password)
    {
        try
        {
            // Hash the password using SHA256 (IBKR requires hashed password)
            var hashedPassword = ComputeSha256Hash(password);
            
            // Form data as expected by IBKR API
            var formData = new Dictionary<string, string>
            {
                { "ACTION", "INIT" },
                { "USER", username },
                { "A", hashedPassword },
                { "RESP_TYPE", "JSON" },
                { "LOGIN_TYPE", "1" },
                { "SERVICE", "AM.LOGIN" }
            };
            
            var content = new FormUrlEncodedContent(formData);
            
            var request = new HttpRequestMessage(HttpMethod.Post, SsoAuthPath)
            {
                Content = content
            };
            
            // Add headers from HAR file
            request.Headers.TryAddWithoutValidation("Accept", "application/json, text/javascript, */*; q=0.01");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            request.Headers.TryAddWithoutValidation("Origin", "https://www.interactivebrokers.com.au");
            request.Headers.TryAddWithoutValidation("Referer", "https://www.interactivebrokers.com.au/sso/Login?RL=1&locale=en_US");
            request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
            request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"144\", \"Google Chrome\";v=\"144\"");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
            request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
            request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
            
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<IbkrAuthResponse>();
                
                if (authResponse != null)
                {
                    _logger.LogInformation("Authentication initiated for user: {Username}", username);
                    return authResponse;
                }
            }

            _logger.LogWarning("Authentication initialization failed for user: {Username}, Status: {StatusCode}", 
                username, response.StatusCode);
            
            return new IbkrAuthResponse 
            { 
                Authenticated = false,
                Message = $"Authentication failed with status: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing authentication for user: {Username}", username);
            throw;
        }
    }
    
    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256Hash = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }

    public async Task<IbkrAuthResponse> PollAuthenticationStatusAsync()
    {
        try
        {
            // Polling uses COMPLETEAUTH action with empty content
            // Note: The actual polling implementation may need session-specific data
            // For now, using a simple POST to match the HAR pattern
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "ACTION", "COMPLETEAUTH" },
                { "RESP_TYPE", "JSON" },
                { "VERSION", "1" },
                { "LOGIN_TYPE", "1" }
            });
            
            var request = new HttpRequestMessage(HttpMethod.Post, SsoAuthPath)
            {
                Content = content
            };
            
            // Add headers from HAR file
            request.Headers.TryAddWithoutValidation("Accept", "application/json, text/javascript, */*; q=0.01");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            request.Headers.TryAddWithoutValidation("Origin", "https://www.interactivebrokers.com.au");
            request.Headers.TryAddWithoutValidation("Referer", "https://www.interactivebrokers.com.au/sso/Login?RL=1&locale=en_US");
            request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
            request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"144\", \"Google Chrome\";v=\"144\"");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
            request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
            request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<IbkrAuthResponse>();
                
                if (authResponse != null)
                {
                    _logger.LogDebug("Authentication poll result - Authenticated: {Authenticated}, Connected: {Connected}", 
                        authResponse.Authenticated, authResponse.Connected);
                    return authResponse;
                }
            }

            _logger.LogWarning("Authentication polling failed, Status: {StatusCode}", response.StatusCode);
            
            return new IbkrAuthResponse 
            { 
                Authenticated = false,
                Message = $"Polling failed with status: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error polling authentication status");
            throw;
        }
    }

    public async Task<IbkrSsoValidateResponse?> ValidateSsoAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, SsoValidatePath);
            
            // Add headers from HAR file
            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            request.Headers.TryAddWithoutValidation("Referer", "https://www.interactivebrokers.com.au/portal/?loginType=1&action=ACCT_MGMT_MAIN&clt=0&RL=1&locale=en_US");
            request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"144\", \"Google Chrome\";v=\"144\"");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
            request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
            request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
            
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var validateResponse = await response.Content.ReadFromJsonAsync<IbkrSsoValidateResponse>();
                _logger.LogInformation("SSO validation result - Valid: {Valid}", validateResponse?.Valid);
                return validateResponse;
            }

            _logger.LogWarning("SSO validation failed, Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating SSO");
            return null;
        }
    }

    public async Task<IbkrAccountsResponse?> GetAccountsAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, AccountsPath);
            
            // Add headers from HAR file
            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            request.Headers.TryAddWithoutValidation("Referer", "https://www.interactivebrokers.com.au/portal/?loginType=1&action=ACCT_MGMT_MAIN&clt=0&RL=1&locale=en_US");
            request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"144\", \"Google Chrome\";v=\"144\"");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
            request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
            request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
            
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IbkrAccountsResponse>();
            }

            _logger.LogWarning("Failed to get accounts, Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accounts");
            return null;
        }
    }

    public async Task<IbkrPortfolioSummary?> GetPortfolioSummaryAsync(string accountId)
    {
        try
        {
            var url = string.Format(PortfolioSummaryPath, accountId);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            // Add headers from HAR file
            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            request.Headers.TryAddWithoutValidation("Referer", "https://www.interactivebrokers.com.au/portal/?loginType=1&action=ACCT_MGMT_MAIN&clt=0&RL=1&locale=en_US");
            request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"144\", \"Google Chrome\";v=\"144\"");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
            request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
            request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
            
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IbkrPortfolioSummary>();
            }

            _logger.LogWarning("Failed to get portfolio summary for account: {AccountId}, Status: {StatusCode}", 
                accountId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting portfolio summary for account: {AccountId}", accountId);
            return null;
        }
    }

    public async Task<List<IbkrPosition>?> GetPositionsAsync(string accountId)
    {
        try
        {
            var url = string.Format(PositionsPath, accountId);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            // Add headers from HAR file
            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            request.Headers.TryAddWithoutValidation("Referer", "https://www.interactivebrokers.com.au/portal/?loginType=1&action=ACCT_MGMT_MAIN&clt=0&RL=1&locale=en_US");
            request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"144\", \"Google Chrome\";v=\"144\"");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
            request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
            request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
            
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // IBKR returns array directly, not wrapped in object
                var positions = await response.Content.ReadFromJsonAsync<List<IbkrPosition>>();
                return positions;
            }

            _logger.LogWarning("Failed to get positions for account: {AccountId}, Status: {StatusCode}", 
                accountId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting positions for account: {AccountId}", accountId);
            return null;
        }
    }

    public async Task<List<IbkrSecurityDefinition>?> GetSecurityDefinitionsAsync(List<int> conIds)
    {
        try
        {
            // Format payload as per HAR file: conids as string array, contracts as boolean
            var payload = new
            {
                conids = conIds.Select(c => c.ToString()).ToArray(),
                contracts = false
            };
            
            var content = JsonContent.Create(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, SecDefPath)
            {
                Content = content
            };
            
            // Add headers from HAR file
            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            request.Headers.TryAddWithoutValidation("Origin", "https://www.interactivebrokers.com.au");
            request.Headers.TryAddWithoutValidation("Referer", "https://www.interactivebrokers.com.au/portal/?loginType=1&action=ACCT_MGMT_MAIN&clt=0&RL=1&locale=en_US");
            request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"144\", \"Google Chrome\";v=\"144\"");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
            request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
            request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
            
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var secDefResponse = await response.Content.ReadFromJsonAsync<IbkrSecDefResponse>();
                return secDefResponse?.Secdef;
            }

            _logger.LogWarning("Failed to get security definitions, Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security definitions");
            return null;
        }
    }
}
