using PortfolioManager.Contracts.Models.Shared;

namespace PortfolioManager.Core.Services.Brokerage;

public interface IBrokerageService : IBrokerageAuthenticationService, IBrokeragePortfolioService
{
    BrokerageType BrokerageType { get; }
}
