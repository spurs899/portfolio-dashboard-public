using Microsoft.Playwright;

namespace PortfolioManager.Core.Services.Brokerage;

public interface IQrAuthenticationService
{
    Task<QrAuthenticationResult> GenerateQrCodeAsync(string username, string password);
    Task<QrAuthenticationResult> CheckAuthenticationStatusAsync(string sessionId);
}

public class QrAuthenticationResult
{
    public string SessionId { get; set; } = string.Empty;
    public byte[]? QrImage { get; set; }
    public bool IsAuthenticated { get; set; }
    public object? SessionData { get; set; }
}

public class IbkrQrAuthenticationService : IQrAuthenticationService
{
    private readonly Dictionary<string, QrAuthenticationContext> _sessions = new();

    private class QrAuthenticationContext
    {
        public IPage? Page { get; set; }
        public IBrowserContext? BrowserContext { get; set; }
        public IBrowser? Browser { get; set; }
        public IPlaywright? Playwright { get; set; }
        public bool IsAuthenticated { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public async Task<QrAuthenticationResult> GenerateQrCodeAsync(string username, string password)
    {
        var sessionId = Guid.NewGuid().ToString();
        var result = new QrAuthenticationResult { SessionId = sessionId };

        try
        {
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            // Navigate to IBKR login page
            await page.GotoAsync("https://ndcdyn.interactivebrokers.com/sso/Login?RL=1&locale=en_US");

            // Enter username and password
            await page.FillAsync("input[name='user_name']", username);
            await page.FillAsync("input[name='password']", password);
            await page.ClickAsync("button[type='submit']");

            // Wait for QR code to appear
            var qrSelector = "img.qr-image, img[src*='qr']";
            await page.WaitForSelectorAsync(qrSelector, new PageWaitForSelectorOptions { Timeout = 15000 });
            var qrElement = await page.QuerySelectorAsync(qrSelector);
            
            if (qrElement == null)
                throw new Exception("QR code not found");
            
            result.QrImage = await qrElement.ScreenshotAsync();

            // Store context for later status checks
            _sessions[sessionId] = new QrAuthenticationContext
            {
                Page = page,
                BrowserContext = context,
                Browser = browser,
                Playwright = playwright
            };

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate QR code: {ex.Message}", ex);
        }
    }

    public async Task<QrAuthenticationResult> CheckAuthenticationStatusAsync(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var context))
        {
            throw new InvalidOperationException("Session not found or expired");
        }

        if (context.Page == null)
        {
            throw new InvalidOperationException("Invalid session state");
        }

        try
        {
            // Check if authentication completed
            var isAuthenticated = await context.Page.Locator("text=Welcome").IsVisibleAsync() ||
                                 await context.Page.Locator(".account-summary").IsVisibleAsync();

            if (isAuthenticated && !context.IsAuthenticated)
            {
                context.IsAuthenticated = true;
                var cookies = await context.BrowserContext!.CookiesAsync();
                
                // Clean up browser resources
                await context.Browser!.CloseAsync();
                context.Playwright?.Dispose();
                
                return new QrAuthenticationResult
                {
                    SessionId = sessionId,
                    IsAuthenticated = true,
                    SessionData = cookies
                };
            }

            return new QrAuthenticationResult
            {
                SessionId = sessionId,
                IsAuthenticated = false
            };
        }
        catch (Exception ex)
        {
            // Clean up on error
            await CleanupSession(sessionId);
            throw new InvalidOperationException($"Failed to check authentication status: {ex.Message}", ex);
        }
    }

    private async Task CleanupSession(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var context))
        {
            try
            {
                if (context.Browser != null)
                    await context.Browser.CloseAsync();
                
                context.Playwright?.Dispose();
            }
            catch
            {
                // Ignore cleanup errors
            }
            
            _sessions.Remove(sessionId);
        }
    }
}
