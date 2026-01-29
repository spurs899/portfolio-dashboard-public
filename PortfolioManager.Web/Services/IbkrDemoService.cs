using PortfolioManager.Contracts.Models;
using PortfolioManager.Contracts.Web;

namespace PortfolioManager.Web.Services;

public class IbkrDemoService : IIbkrService
{
    public async Task<IbkrAuthResult> AuthenticateAsync(string username, string password, Action<string> onQRCodeReceived, Action<string> onStatusUpdate)
    {
        return await Task.FromResult(new IbkrAuthResult());
    }

    public async Task<IbkrAccountsResponse?> GetAccountsAsync()
    {
        return await Task.FromResult(new IbkrAccountsResponse());
    }

    public async Task<IbkrPortfolioSummary?> GetPortfolioSummaryAsync(string accountId)
    {
        return await Task.FromResult(new IbkrPortfolioSummary());
    }

    public async Task<List<InstrumentDto>?> GetPositionsAsync(string accountId)
    {
        return await Task.FromResult(new List<InstrumentDto>())!;
    }

    public async Task<string?> GetStoredUsernameAsync()
    {
        return await Task.FromResult("")!;
    }

    public async Task<bool> ValidateSessionAsync()
    {
        return await Task.FromResult(false)!;
    }

    public async Task ClearSessionAsync()
    {
        await Task.CompletedTask;
    }
}