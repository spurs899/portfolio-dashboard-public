using PortfolioManager.Contracts.Models;

namespace PortfolioManager.Core.Interfaces;

public interface ISharesiesClient
{
    Task<SharesiesLoginResponse> LoginAsync(string email, string password);
    Task<SharesiesProfileResponse?> GetProfileAsync();
    Task<SharesiesPortfolio?> GetPortfolioAsync(string? portfolioId = null);
    Task<SharesiesInstrumentResponse?> GetInstrumentsAsync();
}
