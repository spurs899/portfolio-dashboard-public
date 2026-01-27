using PortfolioManager.Contracts.Models;
using PortfolioManager.Contracts.Web;

namespace PortfolioManager.Web.Services;

public class IbkrDemoService : IIbkrService
{
    public Task<IbkrAuthResult> AuthenticateAsync(string username, string password, Action<string> onQRCodeReceived, Action<string> onStatusUpdate)
    {
        return null;
    }

    public Task<IbkrAccountsResponse?> GetAccountsAsync()
    {
        return null;
    }

    public Task<IbkrPortfolioSummary?> GetPortfolioSummaryAsync(string accountId)
    {
        return null;
    }

    public async Task<List<InstrumentDto>?> GetPositionsAsync(string accountId)
    {
        return new List<InstrumentDto>();
    }

    public async Task<string?> GetStoredUsernameAsync()
    {
        return "spurs899";
    }

    public async Task<bool> ValidateSessionAsync()
    {
        return false;
    }

    public async Task ClearSessionAsync()
    {
        //TODO: no op as demo service
    }
}