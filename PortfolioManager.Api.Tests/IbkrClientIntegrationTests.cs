using Microsoft.Extensions.Configuration;
using FluentAssertions;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioManager.Core.Services;

namespace PortfolioManager.Api.Tests;

public class IbkrClientIntegrationTests
{
    private readonly IIbkrClient _ibkrClient;
    private readonly string? _username;
    private readonly string? _password;

    public IbkrClientIntegrationTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        _username = configuration["Ibkr:Username"] ?? Environment.GetEnvironmentVariable("IBKR_USERNAME");
        _password = configuration["Ibkr:Password"] ?? Environment.GetEnvironmentVariable("IBKR_PASSWORD");

        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };
        var httpClient = new HttpClient(handler);
        var loggerMock = new Mock<ILogger<IbkrClient>>();
        _ibkrClient = new IbkrClient(httpClient, loggerMock.Object);
    }

    [Fact(Skip = "Requires actual IBKR credentials and QR code authentication")]
    public async Task InitializeAuthentication_ShouldReturnAuthResponse()
    {
        if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
        {
            return;
        }

        // 1. Initialize authentication
        var authResponse = await _ibkrClient.InitializeAuthenticationAsync(_username, _password);
        authResponse.Should().NotBeNull("Authentication initialization should return a response");
        
        // Note: This will typically not be authenticated immediately as it requires QR code scan
        // The response should indicate that authentication is pending
    }

    [Fact(Skip = "Requires actual IBKR credentials and QR code authentication")]
    public async Task FullIntegrationFlow_ShouldSucceed()
    {
        if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
        {
            return;
        }

        // 1. Initialize authentication
        var authResponse = await _ibkrClient.InitializeAuthenticationAsync(_username, _password);
        authResponse.Should().NotBeNull("Authentication initialization should return a response");

        // 2. Poll for authentication (requires manual QR code scan)
        // In a real scenario, you would need to scan the QR code on your mobile device
        // and then poll until authenticated
        const int maxAttempts = 30;
        bool authenticated = false;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            var pollResponse = await _ibkrClient.PollAuthenticationStatusAsync();
            
            if (pollResponse.Authenticated)
            {
                authenticated = true;
                break;
            }

            await Task.Delay(1000); // Wait 1 second between polls
        }

        if (!authenticated)
        {
            throw new Exception("Authentication timeout. Please scan the QR code within 30 seconds and re-run the test.");
        }

        // 3. Validate SSO
        var validateResponse = await _ibkrClient.ValidateSsoAsync();
        validateResponse.Should().NotBeNull("SSO validation should return a response");
        validateResponse!.Valid.Should().BeTrue("SSO should be valid after authentication");

        // 4. Get Accounts
        var accountsResponse = await _ibkrClient.GetAccountsAsync();
        accountsResponse.Should().NotBeNull("Accounts should be retrieved after authentication");
        accountsResponse!.Accounts.Should().NotBeNull().And.NotBeEmpty("At least one account should exist");

        var accountId = accountsResponse.Accounts![0].AccountId;
        accountId.Should().NotBeNullOrEmpty("Account ID should not be empty");

        // 5. Get Portfolio Summary
        var portfolioSummary = await _ibkrClient.GetPortfolioSummaryAsync(accountId!);
        portfolioSummary.Should().NotBeNull("Portfolio summary should be retrieved");

        // 6. Get Positions
        var positions = await _ibkrClient.GetPositionsAsync(accountId!);
        positions.Should().NotBeNull("Positions should be retrieved");
        
        if (positions!.Count > 0)
        {
            // 7. Get Security Definitions (if positions exist)
            var conIds = positions
                .Where(p => p.ConId.HasValue)
                .Select(p => p.ConId!.Value)
                .Distinct()
                .ToList();

            if (conIds.Count > 0)
            {
                var secDefs = await _ibkrClient.GetSecurityDefinitionsAsync(conIds);
                secDefs.Should().NotBeNull("Security definitions should be retrieved");
                secDefs!.Count.Should().BeGreaterThan(0, "At least one security definition should be returned");
            }
        }
    }

    [Fact(Skip = "Requires active IBKR session")]
    public async Task ValidateSso_WithoutAuthentication_ShouldFail()
    {
        // Attempt to validate SSO without authentication
        var validateResponse = await _ibkrClient.ValidateSsoAsync();
        
        // Should either return null or indicate invalid session
        if (validateResponse != null)
        {
            validateResponse.Valid.Should().BeFalse("SSO should not be valid without authentication");
        }
    }

    [Fact(Skip = "Requires active IBKR session")]
    public async Task GetAccounts_WithoutAuthentication_ShouldReturnNull()
    {
        // Attempt to get accounts without authentication
        var accountsResponse = await _ibkrClient.GetAccountsAsync();
        
        // Should return null when not authenticated
        accountsResponse.Should().BeNull("Accounts should not be accessible without authentication");
    }

    [Fact(Skip = "Requires active IBKR session with positions")]
    public async Task GetPositions_WithValidAccountId_ShouldReturnPositions()
    {
        // This test assumes authentication is already done
        // and you have an active session with positions
        
        const string testAccountId = ""; // Add your account ID here for manual testing
        
        if (string.IsNullOrEmpty(testAccountId))
        {
            return;
        }

        var positions = await _ibkrClient.GetPositionsAsync(testAccountId);
        positions.Should().NotBeNull("Positions should be retrieved with valid account ID");
    }
}
