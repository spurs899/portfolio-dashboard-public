using Microsoft.AspNetCore.SignalR;

namespace PortfolioManager.Api.Hubs;

/// <summary>
/// SignalR hub for real-time IBKR authentication updates
/// </summary>
public class IbkrAuthHub : Hub
{
    /// <summary>
    /// Register connection for receiving auth updates
    /// </summary>
    public async Task RegisterForAuth(string username)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"auth_{username}");
    }

    /// <summary>
    /// Unregister from auth updates
    /// </summary>
    public async Task UnregisterFromAuth(string username)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"auth_{username}");
    }
}
