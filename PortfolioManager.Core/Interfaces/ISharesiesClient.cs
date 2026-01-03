using PortfolioManager.Contracts.Models;

namespace PortfolioManager.Core.Interfaces;

public interface ISharesiesClient
{
    Task<bool> LoginAsync(string email, string password);
    Task<SharesiesProfile?> GetProfileAsync();
    Task<SharesiesPortfolio?> GetPortfolioAsync();
}
