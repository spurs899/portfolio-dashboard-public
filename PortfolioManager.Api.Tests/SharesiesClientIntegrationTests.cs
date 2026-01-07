using Microsoft.Extensions.Configuration;
using FluentAssertions;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using PortfolioManager.Contracts.Models;
using PortfolioManager.Core.Services;

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
        const string mfaCode = "";
        SharesiesLoginResponse loginResult;
        if (string.IsNullOrEmpty(mfaCode))
        {
            loginResult = await _sharesiesClient.LoginAsync(_email, _password);
            if (loginResult is { Type: "identity_email_mfa_required" })
            {
                throw new Exception("Retrieve MFA code and update mfaCode variable and re-run this test");
            }
        }
        else
        {
            loginResult = await _sharesiesClient.LoginAsync(_email, _password, mfaCode);
        }
        
        loginResult.Should().NotBeNull("Login should succeed with valid credentials");
        var userId = loginResult.User.Id;
        var rakaiaToken = loginResult.RakaiaToken;
        var distillToken = loginResult.DistillToken;

        // 2. Get Profile
        var profileResponse = await _sharesiesClient.GetProfileAsync();
        profileResponse.Should().NotBeNull("Profile should be retrieved after login");
        profileResponse!.Profiles.Should().NotBeNull().And.NotBeEmpty();
        profileResponse.Profiles![0].Name.Should().NotBeNullOrEmpty();

        // 3. Get Portfolio
        var sharesiesProfile = profileResponse.Profiles.First();
        var sharesiesProfilePortfolio = sharesiesProfile.Portfolios.First(x => x.Product == "INVEST");
        var portfolio = await _sharesiesClient.GetPortfolioAsync(userId, sharesiesProfilePortfolio.Id, rakaiaToken);
        portfolio.Should().NotBeNull("Portfolio should be retrieved after login");
        portfolio!.InstrumentReturns.Should().NotBeNull();

        // 4. Get Instruments
        var instrumentIds = portfolio.InstrumentReturns?.Keys.ToList() ?? new List<string>();
        var sharesiesInstrumentResponse = await _sharesiesClient.GetInstrumentsAsync(userId, instrumentIds, distillToken);
        sharesiesInstrumentResponse.Should().NotBeNull("Instruments should be retrieved after login");
        sharesiesInstrumentResponse!.Instruments.Should().NotBeNullOrEmpty();
    }
}
