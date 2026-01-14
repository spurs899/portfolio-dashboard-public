using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PortfolioManager.Contracts;
using PortfolioManager.Contracts.Models;
using PortfolioManager.Core.Extensions;

namespace PortfolioManager.Core.Services;

public interface ISharesiesAuthClient
{
    Task<SharesiesLoginResponse> LoginAsync(string email, string password, string? mfaCode = null);
}

public interface ISharesiesDataClient
{
    Task<SharesiesProfileResponse?> GetProfileAsync();
    Task<SharesiesPortfolio?> GetPortfolioAsync(string userId, string portfolioId, string rakaiaToken);
    Task<SharesiesInstrumentResponse?> GetInstrumentsAsync(string userId, List<string> instrumentIds, string distillToken);
}

// Composite interface for backward compatibility
public interface ISharesiesClient : ISharesiesAuthClient, ISharesiesDataClient
{
}

public class SharesiesClient : ISharesiesClient
{
    private const string MfaRequiredType = "identity_email_mfa_required";
    private readonly HttpClient _httpClient;
    private readonly ILogger<SharesiesClient> _logger;

    public SharesiesClient(HttpClient httpClient, ILogger<SharesiesClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.DefaultRequestHeaders.Add(CoreConstants.UserAgent, CoreConstants.UserAgentValue);
    }

    public async Task<SharesiesLoginResponse> LoginAsync(string email, string password, string? mfaCode = null)
    {
        try
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

                if (loginResponse?.Type == MfaRequiredType && string.IsNullOrEmpty(mfaCode))
                {
                    _logger.LogInformation("MFA required for user: {Email}", email);
                    return loginResponse;
                }

                if (loginResponse is { Authenticated: true })
                {
                    _logger.LogInformation("Successfully authenticated user: {Email}", email);
                    return loginResponse;
                }
            }
            
            _logger.LogWarning("Login failed for user: {Email}, Status: {StatusCode}", email, response.StatusCode);
            throw new UnauthorizedAccessException($"Sharesies - Unable to login for user: {email}");
        }
        catch (Exception ex) when (ex is not UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Error during Sharesies login for user: {Email}", email);
            throw;
        }
    }

    public async Task<SharesiesProfileResponse?> GetProfileAsync()
    {
        return await _httpClient.GetFromJsonAsync<SharesiesProfileResponse>($"{Constants.BaseSharesiesApiUrl}/profiles");
    }

    public async Task<SharesiesPortfolio?> GetPortfolioAsync(string userId, string portfolioId, string rakaiaToken)
    {
        try
        {
            var url = $"{Constants.BasePortfolioSharesiesApiUrl}/portfolios/{portfolioId}/instruments";
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.AddCommonBrowserHeaders();
            request.AddBearerToken(rakaiaToken);
            request.AddSharesiesBrowserHeaders(Constants.Origin);
            request.Headers.TryAddWithoutValidation(CoreConstants.UserAgent, CoreConstants.UserAgentValue);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SharesiesPortfolio>();
            }

            _logger.LogWarning("Failed to get portfolio for user: {UserId}, Status: {StatusCode}", userId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting portfolio for user: {UserId}", userId);
            return null;
        }
    }

    public async Task<SharesiesInstrumentResponse?> GetInstrumentsAsync(string userId, List<string> instrumentIds, string distillToken)
    {
        try
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

            request.AddCommonBrowserHeaders();
            request.AddBearerToken(distillToken);
            request.AddSharesiesBrowserHeaders(Constants.Origin);
            request.Headers.TryAddWithoutValidation(CoreConstants.UserAgent, CoreConstants.UserAgentValue);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SharesiesInstrumentResponse>();
            }

            _logger.LogWarning("Failed to get instruments for user: {UserId}, Status: {StatusCode}", userId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting instruments for user: {UserId}", userId);
            return null;
        }
    }
}

