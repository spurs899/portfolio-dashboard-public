using System.Net;
using System.Net.Http.Json;
using Moq;
using Moq.Protected;
using PortfolioManager.Api.Services;
using FluentAssertions;
using PortfolioManager.Contracts.Models;

namespace PortfolioManager.Api.Tests;

public class SharesiesClientTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly SharesiesClient _sut;

    public SharesiesClientTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://app.sharesies.nz/api/")
        };
        _sut = new SharesiesClient(_httpClient);
    }

    [Fact]
    public async Task LoginAsync_ReturnsTrue_WhenSuccessful()
    {
        // Arrange
        var loginResponse = new SharesiesLoginResponse
        {
            AuthenticatedUser = new SharesiesAuthenticatedUser { Token = "token", UserId = "user", Email = "test@test.com" }
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
        var result = await _sut.LoginAsync("test@test.com", "password");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetProfileAsync_ReturnsProfile_WhenSuccessful()
    {
        // Arrange
        var profile = new SharesiesProfile { User = new SharesiesUser { Id = "1", Email = "test@test.com" } };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.AbsolutePath.Contains("identity/profile")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(profile)
            });

        // Act
        var result = await _sut.GetProfileAsync();

        // Assert
        result.Should().NotBeNull();
        result!.User!.Email.Should().Be("test@test.com");
    }
}
