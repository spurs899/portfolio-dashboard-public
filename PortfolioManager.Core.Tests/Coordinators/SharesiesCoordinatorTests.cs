using FluentAssertions;
using Moq;
using PortfolioManager.Contracts.Models;
using PortfolioManager.Contracts.Models.Shared;
using PortfolioManager.Core.Coordinators;
using PortfolioManager.Core.Services;

namespace PortfolioManager.Core.Tests.Coordinators;

public class SharesiesCoordinatorTests
{
    private readonly Mock<ISharesiesClient> _mockSharesiesClient;
    private readonly SharesiesCoordinator _coordinator;

    public SharesiesCoordinatorTests()
    {
        _mockSharesiesClient = new Mock<ISharesiesClient>();
        _coordinator = new SharesiesCoordinator(_mockSharesiesClient.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnLoginResponse_WhenCalled()
    {
        const string email = "test@example.com";
        const string password = "password123";
        var expectedResponse = new SharesiesLoginResponse
        {
            Authenticated = true,
            RakaiaToken = "test-rakaia-token",
            DistillToken = "test-distill-token"
        };

        _mockSharesiesClient
            .Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedResponse);

        var result = await _coordinator.Login(email, password);

        result.Should().NotBeNull();
        result.Authenticated.Should().BeTrue();
        result.RakaiaToken.Should().Be("test-rakaia-token");
        result.DistillToken.Should().Be("test-distill-token");
        _mockSharesiesClient.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoginProvideMfaCode_ShouldReturnLoginResponse_WhenCalled()
    {
        const string email = "test@example.com";
        const string password = "password123";
        const string mfaCode = "123456";
        var expectedResponse = new SharesiesLoginResponse
        {
            Authenticated = true,
            RakaiaToken = "test-rakaia-token",
            DistillToken = "test-distill-token"
        };

        _mockSharesiesClient
            .Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedResponse);

        var result = await _coordinator.LoginProvideMfaCode(email, password, mfaCode);

        result.Should().NotBeNull();
        result.Authenticated.Should().BeTrue();
        _mockSharesiesClient.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnProfileResponse_WhenCalled()
    {
        var expectedResponse = new SharesiesProfileResponse
        {
            Profiles = [new SharesiesProfile { Id = "profile-123", Name = "Test Profile" }]
        };

        _mockSharesiesClient.Setup(x => x.GetProfileAsync()).ReturnsAsync(expectedResponse);

        var result = await _coordinator.GetProfile();

        result.Should().NotBeNull();
        result.Profiles.Should().HaveCount(1);
        _mockSharesiesClient.Verify(x => x.GetProfileAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAggregatedProfileAndInstrumentsAsync_ShouldReturnNulls_WhenProfileResponseIsNull()
    {
        _mockSharesiesClient.Setup(x => x.GetProfileAsync()).ReturnsAsync((SharesiesProfileResponse?)null);

        var (userProfile, instruments) = await _coordinator.GetAggregatedProfileAndInstrumentsAsync("user-123", "rakaia", "distill");

        userProfile.Should().BeNull();
        instruments.Should().BeNull();
    }

    [Fact]
    public async Task GetAggregatedProfileAndInstrumentsAsync_ShouldReturnAggregatedData_WhenValidDataProvided()
    {
        const string userId = "user-123";
        const string portfolioId = "portfolio-123";
        const string rakaiaToken = "rakaia-token";
        const string distillToken = "distill-token";

        var profileResponse = new SharesiesProfileResponse
        {
            Profiles =
            [
                new SharesiesProfile
                {
                    Id = "profile-123",
                    Name = "Test Profile",
                    Portfolios =
                    [
                        new SharesiesProfilePortfolio
                        {
                            Id = portfolioId,
                            Product = "INVEST",
                            Image = "https://example.com/image.png"
                        }
                    ]
                }
            ]
        };

        var portfolioResponse = new SharesiesPortfolio
        {
            InstrumentReturns = new Dictionary<string, SharesiesInstrumentReturn>
            {
                { "instrument-1", new SharesiesInstrumentReturn { InstrumentUuid = "instrument-1", SharesOwned = 100m, InvestmentValue = 5000m, CostBasis = 4500m, TotalReturn = 500m, SimpleReturn = 50m, DividendsReceived = 25m } }
            }
        };

        var instrumentsResponse = new SharesiesInstrumentResponse
        {
            Instruments =
            [
                new SharesiesInstrument
                {
                    Id = "instrument-1", Symbol = "AAPL", Name = "Apple Inc.", Currency = "USD", MarketPrice = "50.00"
                }
            ]
        };

        _mockSharesiesClient.Setup(x => x.GetProfileAsync()).ReturnsAsync(profileResponse);
        _mockSharesiesClient.Setup(x => x.GetPortfolioAsync(userId, portfolioId, rakaiaToken)).ReturnsAsync(portfolioResponse);
        _mockSharesiesClient.Setup(x => x.GetInstrumentsAsync(userId, It.IsAny<List<string>>(), distillToken)).ReturnsAsync(instrumentsResponse);

        var (userProfile, instruments) = await _coordinator.GetAggregatedProfileAndInstrumentsAsync(userId, rakaiaToken, distillToken);

        userProfile.Should().NotBeNull();
        userProfile!.BrokerageType.Should().Be(BrokerageType.Sharesies);
        instruments.Should().HaveCount(1);
        instruments![0].Symbol.Should().Be("AAPL");
    }
}
