using PortfolioManager.Contracts.Models.Brokerage;

namespace PortfolioManager.Core.Services.Brokerage;

public interface IBrokeragePortfolioService
{
    Task<PortfolioData?> GetPortfolioDataAsync(AuthenticationResult authResult);
}
