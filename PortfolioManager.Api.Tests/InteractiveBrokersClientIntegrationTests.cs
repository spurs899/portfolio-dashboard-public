using Microsoft.Extensions.Configuration;
using FluentAssertions;
using System.Net;
using PortfolioManager.Core.Services;
using Xunit;

namespace PortfolioManager.Api.Tests;

public class InteractiveBrokersClientIntegrationTests
{
    private readonly IInteractiveBrokersClient _ibClient;
    private readonly string? _username;
    private readonly string? _password;
    private string? _accountId;

    public InteractiveBrokersClientIntegrationTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        _username = configuration["IBKR:Username"] ?? Environment.GetEnvironmentVariable("IBKR_USERNAME");
        _password = configuration["IBKR:Password"] ?? Environment.GetEnvironmentVariable("IBKR_PASSWORD");

        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };
        var httpClient = new HttpClient(handler);
        _ibClient = new InteractiveBrokersClient(httpClient);
    }

    [Fact]
    public async Task FullIntegrationFlow_ShouldSucceed()
    {
        if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
        {
            return;
        }

        // 1. Authenticate
        var loginResult = await _ibClient.AuthenticateAsync(_username, _password);
        loginResult.Should().NotBeNull("Authentication should succeed with valid credentials");
        loginResult!.Authenticated.Should().BeTrue();

        // 2. Validate session
        var isValid = await _ibClient.ValidateSessionAsync();
        isValid.Should().BeTrue("Session should be valid after authentication");

        // 3. Get account info
        var account = await _ibClient.GetAccountAsync();
        account.Should().NotBeNull("Account info should be retrieved after authentication");
        _accountId = account!.UserId;
        _accountId.Should().NotBeNullOrEmpty();

        // 4. Get positions
        var positions = await _ibClient.GetPositionsAsync(_accountId!);
        positions.Should().NotBeNull("Positions should be retrieved for the account");
    }
}
