using Microsoft.Extensions.Configuration;
using PortfolioManager.Api.Services;
using FluentAssertions;
using System.Net;
using PortfolioManager.Core.Interfaces;

namespace PortfolioManager.Api.Tests;

public class SharesiesClientIntegrationTests
{
    private readonly ISharesiesClient _sharesiesClient;
    private readonly string? _email;
    private readonly string? _password;

    public SharesiesClientIntegrationTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        _email = configuration["Sharesies:Email"] ?? Environment.GetEnvironmentVariable("SHARESIES_EMAIL");
        _password = configuration["Sharesies:Password"] ?? Environment.GetEnvironmentVariable("SHARESIES_PASSWORD");

        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };
        var httpClient = new HttpClient(handler);
        _sharesiesClient = new SharesiesClient(httpClient);
    }

    //[Fact(Skip = "Requires actual Sharesies credentials")]
    [Fact]
    public async Task FullIntegrationFlow_ShouldSucceed()
    {
        if (string.IsNullOrEmpty(_email) || string.IsNullOrEmpty(_password))
        {
            return;
        }

        // 1. Login
        var loginResult = await _sharesiesClient.LoginAsync(_email, _password);
        loginResult.Should().NotBeNull("Login should succeed with valid credentials");

        // 2. Get Profile
        var profileResponse = await _sharesiesClient.GetProfileAsync();
        profileResponse.Should().NotBeNull("Profile should be retrieved after login");
        profileResponse!.Profiles.Should().NotBeNull().And.NotBeEmpty();
        profileResponse.Profiles![0].Name.Should().NotBeNullOrEmpty();

        // 3. Get Portfolio
        var sharesiesProfile = profileResponse.Profiles.First();
        var sharesiesProfilePortfolio = sharesiesProfile.Portfolios.First(x => x.Product == "INVEST");
        var portfolio = await _sharesiesClient.GetPortfolioAsync(sharesiesProfilePortfolio.Id);
        portfolio.Should().NotBeNull("Portfolio should be retrieved after login");
        portfolio!.InstrumentReturns.Should().NotBeNull();

        var sharesiesInstrumentResponse = await _sharesiesClient.GetInstrumentsAsync();
        sharesiesInstrumentResponse.Should().NotBeNull();
    }
}
