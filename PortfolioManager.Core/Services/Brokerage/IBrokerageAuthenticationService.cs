using PortfolioManager.Contracts.Models.Brokerage;

namespace PortfolioManager.Core.Services.Brokerage;

public interface IBrokerageAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(AuthenticationCredentials credentials);
    Task<AuthenticationResult> ContinueAuthenticationAsync(AuthenticationCredentials credentials);
    Task<bool> ValidateSessionAsync(string sessionId);
}
