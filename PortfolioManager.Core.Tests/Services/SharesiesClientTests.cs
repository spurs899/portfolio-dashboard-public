using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PortfolioManager.Contracts.Models;
using PortfolioManager.Core.Services;

namespace PortfolioManager.Core.Tests.Services;

public class SharesiesClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly SharesiesClient _client;

    public SharesiesClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        var loggerMock = new Mock<ILogger<SharesiesClient>>();
        _client = new SharesiesClient(httpClient, loggerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnLoginResponse_WhenCredentialsAreValid()
    {
        var expectedResponse = new SharesiesLoginResponse
        {
            Authenticated = true,
            RakaiaToken = "test-rakaia",
            DistillToken = "test-distill"
        };

        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        var result = await _client.LoginAsync("test@example.com", "password");

        result.Should().NotBeNull();
        result.Authenticated.Should().BeTrue();
        result.RakaiaToken.Should().Be("test-rakaia");
        result.DistillToken.Should().Be("test-distill");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnMfaRequired_WhenMfaIsNeeded()
    {
        var expectedResponse = new SharesiesLoginResponse
        {
            Authenticated = false,
            Type = "identity_email_mfa_required"
        };

        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        var result = await _client.LoginAsync("test@example.com", "password");

        result.Should().NotBeNull();
        result.Type.Should().Be("identity_email_mfa_required");
        result.Authenticated.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_WithMfaCode_ShouldReturnLoginResponse()
    {
        var expectedResponse = new SharesiesLoginResponse
        {
            Authenticated = true,
            RakaiaToken = "test-rakaia",
            DistillToken = "test-distill"
        };

        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        var result = await _client.LoginAsync("test@example.com", "password", "123456");

        result.Should().NotBeNull();
        result.Authenticated.Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenCredentialsAreInvalid()
    {
        SetupHttpResponse(HttpStatusCode.Unauthorized, new object());

        var act = async () => await _client.LoginAsync("test@example.com", "wrong-password");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Sharesies - Unable to login for user: test@example.com");
    }

    [Fact]
    public async Task GetProfileAsync_ShouldReturnProfile_WhenSuccessful()
    {
        var expectedResponse = new SharesiesProfileResponse
        {
            Profiles = [new SharesiesProfile { Id = "profile-123", Name = "Test Profile" }]
        };

        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        var result = await _client.GetProfileAsync();

        result.Should().NotBeNull();
        result!.Profiles.Should().HaveCount(1);
        result.Profiles![0].Id.Should().Be("profile-123");
    }

    [Fact]
    public async Task GetPortfolioAsync_ShouldReturnPortfolio_WhenSuccessful()
    {
        var expectedResponse = new SharesiesPortfolio
        {
            InstrumentReturns = new Dictionary<string, SharesiesInstrumentReturn>
            {
                { "inst-1", new SharesiesInstrumentReturn { InstrumentUuid = "inst-1", SharesOwned = 100m } }
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        var result = await _client.GetPortfolioAsync("user-123", "portfolio-123", "rakaia-token");

        result.Should().NotBeNull();
        result!.InstrumentReturns.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPortfolioAsync_ShouldReturnNull_WhenRequestFails()
    {
        SetupHttpResponse(HttpStatusCode.Unauthorized, new object());

        var result = await _client.GetPortfolioAsync("user-123", "portfolio-123", "invalid-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInstrumentsAsync_ShouldReturnInstruments_WhenSuccessful()
    {
        var expectedResponse = new SharesiesInstrumentResponse
        {
            Instruments = [new SharesiesInstrument { Id = "inst-1", Symbol = "AAPL", Name = "Apple Inc." }]
        };

        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        var result = await _client.GetInstrumentsAsync("user-123", new List<string> { "inst-1" }, "distill-token");

        result.Should().NotBeNull();
        result!.Instruments.Should().HaveCount(1);
        result.Instruments![0].Symbol.Should().Be("AAPL");
    }

    [Fact]
    public async Task GetInstrumentsAsync_ShouldReturnNull_WhenRequestFails()
    {
        SetupHttpResponse(HttpStatusCode.BadRequest, new object());

        var result = await _client.GetInstrumentsAsync("user-123", new List<string> { "inst-1" }, "invalid-token");

        result.Should().BeNull();
    }

    private void SetupHttpResponse<T>(HttpStatusCode statusCode, T content)
    {
        var json = JsonSerializer.Serialize(content);
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);
    }
}
