using System.Net;
using System.Net.Http.Json;
using Moq;
using Moq.Protected;
using PortfolioManager.Api.Services;
using FluentAssertions;
using PortfolioManager.Contracts.Models;
using PortfolioManager.Core.Interfaces;

namespace PortfolioManager.Api.Tests;

public class SharesiesClientTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly ISharesiesClient _sharesiesClient;

    public SharesiesClientTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://app.sharesies.nz/api/")
        };
        _sharesiesClient = new SharesiesClient(_httpClient);
    }

    [Fact]
    public async Task LoginAsync_ReturnsTrue_WhenSuccessful()
    {
        // Arrange
        var loginResponse = new SharesiesLoginResponse
        {
            Authenticated = true,
            User = new SharesiesUser { Id = "user", Email = "test@test.com" }
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(loginResponse)
            });

        // Act
        var result = await _sharesiesClient.LoginAsync("test@test.com", "password");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProfileAsync_ReturnsProfile_WhenSuccessful()
    {
        // Arrange
        var profileResponse = new SharesiesProfileResponse
        {
            Profiles = new List<SharesiesProfile>
            {
                new SharesiesProfile { Id = "1", Name = "Paul" }
            }
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.AbsolutePath.Contains("profiles")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(profileResponse)
            });

        // Act
        var result = await _sharesiesClient.GetProfileAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Profiles.Should().NotBeNull().And.HaveCount(1);
        result.Profiles![0].Name.Should().Be("Paul");
    }
}
