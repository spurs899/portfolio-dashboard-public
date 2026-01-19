using Microsoft.AspNetCore.SignalR;
using PortfolioManager.Api.Hubs;
using PortfolioManager.Core.Services;

namespace PortfolioManager.Api.Services;

/// <summary>
/// Implementation of IBKR auth notification service using SignalR
/// </summary>
public class IbkrAuthNotificationService : IIbkrAuthNotificationService
{
    private readonly IHubContext<IbkrAuthHub> _hubContext;
    private readonly ILogger<IbkrAuthNotificationService> _logger;

    public IbkrAuthNotificationService(
        IHubContext<IbkrAuthHub> hubContext,
        ILogger<IbkrAuthNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendQRCodeImageAsync(string connectionId, string base64Image)
    {
        try
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveQRCode", base64Image);
            _logger.LogDebug("Sent QR code image to connection: {ConnectionId}", connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send QR code to connection: {ConnectionId}", connectionId);
        }
    }

    public async Task NotifyAuthStatusAsync(string connectionId, string message)
    {
        try
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveAuthStatus", message);
            _logger.LogDebug("Sent auth status to connection {ConnectionId}: {Message}", connectionId, message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send auth status to connection: {ConnectionId}", connectionId);
        }
    }
}
