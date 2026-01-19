using System.Net;
using PortfolioManager.Core.Services;

namespace PortfolioManager.Api.Handlers;

/// <summary>
/// Custom message handler that injects IBKR session cookies based on X-IBKR-Username header
/// </summary>
public class IbkrSessionHandler : DelegatingHandler
{
    private readonly IIbkrSessionManager _sessionManager;
    private readonly ILogger<IbkrSessionHandler> _logger;

    public IbkrSessionHandler(IIbkrSessionManager sessionManager, ILogger<IbkrSessionHandler> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Check if request has X-IBKR-Username header
        if (request.Headers.TryGetValues("X-IBKR-Username", out var usernames))
        {
            var username = usernames.FirstOrDefault();
            if (!string.IsNullOrEmpty(username))
            {
                // Get session cookies for this user
                var cookieContainer = _sessionManager.GetSessionCookies(username);
                
                if (cookieContainer != null && request.RequestUri != null)
                {
                    // Get all cookies for this domain
                    var cookies = cookieContainer.GetCookies(request.RequestUri);
                    
                    if (cookies.Count > 0)
                    {
                        // Build cookie header string
                        var cookieHeader = string.Join("; ", 
                            cookies.Cast<Cookie>().Select(c => $"{c.Name}={c.Value}"));
                        
                        // Add cookie header to request
                        request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
                        
                        _logger.LogDebug("Injected {Count} cookies for user: {Username}", cookies.Count, username);
                    }
                    else
                    {
                        _logger.LogWarning("No cookies found in container for user: {Username}, URI: {Uri}", 
                            username, request.RequestUri);
                    }
                }
                else
                {
                    _logger.LogWarning("No session found for user: {Username}", username);
                }
            }
        }

        // Proceed with the request
        return await base.SendAsync(request, cancellationToken);
    }
}
