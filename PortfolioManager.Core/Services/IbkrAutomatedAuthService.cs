using Microsoft.Playwright;
using Microsoft.Extensions.Logging;
using System.Net;

namespace PortfolioManager.Core.Services;

/// <summary>
/// Automated IBKR authentication service using Playwright browser automation.
/// Handles the entire login flow including 2FA/QR code authentication.
/// </summary>
public interface IIbkrAutomatedAuthService
{
    /// <summary>
    /// Authenticate with IBKR and return cookies automatically
    /// </summary>
    Task<IbkrAuthenticationResult> AuthenticateAsync(
        string username, 
        string password, 
        string? connectionId = null,
        CancellationToken cancellationToken = default);
}

public class IbkrAutomatedAuthService : IIbkrAutomatedAuthService
{
    private readonly ILogger<IbkrAutomatedAuthService> _logger;
    private readonly IIbkrSessionManager _sessionManager;
    private readonly IIbkrAuthNotificationService? _notificationService;
    
    public IbkrAutomatedAuthService(
        ILogger<IbkrAutomatedAuthService> logger,
        IIbkrSessionManager sessionManager,
        IIbkrAuthNotificationService? notificationService = null)
    {
        _logger = logger;
        _sessionManager = sessionManager;
        _notificationService = notificationService;
    }

    public async Task<IbkrAuthenticationResult> AuthenticateAsync(
        string username, 
        string password,
        string? connectionId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting automated IBKR authentication for user: {Username}", username);

            // Initialize Playwright
            var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false, // Set to true for production, false for debugging
                SlowMo = 100 // Slow down by 100ms for stability
            });

            var context = await browser.NewContextAsync(new()
            {
                ViewportSize = new() { Width = 1280, Height = 720 },
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36"
            });

            var page = await context.NewPageAsync();

            // Navigate to IBKR login
            _logger.LogInformation("Navigating to IBKR login page");
            await page.GotoAsync("https://www.interactivebrokers.com.au/sso/Login?RL=1&locale=en_US");

            // Wait for login form
            await page.WaitForSelectorAsync("input[name='username'], input[name='user']", new() 
            { 
                Timeout = 10000 
            });

            // Fill in username
            _logger.LogInformation("Filling username");
            var usernameInput = page.Locator("input[name='username'], input[name='user']").First;
            await usernameInput.FillAsync(username);

            // Fill in password
            _logger.LogInformation("Filling password");
            var passwordInput = page.Locator("input[name='password'], input[type='password']").First;
            await passwordInput.FillAsync(password);

            // Click login button
            _logger.LogInformation("Clicking login button");
            var loginButton = page.Locator("button[type='submit'], input[type='submit']").First;
            await loginButton.ClickAsync();

            // Wait for 2FA/QR code page to load
            _logger.LogInformation("Waiting for 2FA/QR code page to load...");
            await Task.Delay(3000); // Wait for QR code to render

            // Notify client that we're ready for QR scan
            if (_notificationService != null && !string.IsNullOrEmpty(connectionId))
            {
                await _notificationService.NotifyAuthStatusAsync(connectionId, "QR code ready. Please scan with your phone.");
            }

            var result = new IbkrAuthenticationResult
            {
                Success = false,
                Username = username
            };

            try
            {
                // Start streaming screenshots of QR code to client
                var screenshotCts = new CancellationTokenSource();
                var screenshotTask = Task.Run(async () =>
                {
                    if (_notificationService != null && !string.IsNullOrEmpty(connectionId))
                    {
                        try
                        {
                            // Send initial screenshot
                            var initialScreenshot = await page.ScreenshotAsync(new() 
                            { 
                                Type = ScreenshotType.Png,
                                FullPage = false
                            });
                            await _notificationService.SendQRCodeImageAsync(
                                connectionId, 
                                Convert.ToBase64String(initialScreenshot));

                            // Update screenshot every 3 seconds
                            while (!screenshotCts.Token.IsCancellationRequested)
                            {
                                await Task.Delay(3000, screenshotCts.Token);
                                
                                var screenshot = await page.ScreenshotAsync(new() 
                                { 
                                    Type = ScreenshotType.Png,
                                    FullPage = false
                                });
                                
                                await _notificationService.SendQRCodeImageAsync(
                                    connectionId, 
                                    Convert.ToBase64String(screenshot));
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // Expected when authentication completes
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error streaming QR code screenshots");
                        }
                    }
                }, screenshotCts.Token);

                // Wait up to 90 seconds for user to complete 2FA/QR code
                await page.WaitForURLAsync("**/portal/**", new() 
                { 
                    Timeout = 90000 // 90 seconds for QR code scan
                });

                // Stop screenshot streaming
                screenshotCts.Cancel();
                try { await screenshotTask; } catch { }
                
                _logger.LogInformation("Successfully authenticated! Capturing cookies...");

                // Get cookies from the browser context
                var cookies = await context.CookiesAsync();
                
                // Convert to CookieContainer
                var cookieContainer = new CookieContainer();
                foreach (var cookie in cookies)
                {
                    cookieContainer.Add(new System.Net.Cookie(
                        cookie.Name,
                        cookie.Value,
                        cookie.Path,
                        cookie.Domain)
                    {
                        Secure = cookie.Secure,
                        HttpOnly = cookie.HttpOnly
                    });
                }

                // Store in session manager
                _sessionManager.SetSessionCookies(username, cookieContainer);

                result.Success = true;
                result.Message = "Authentication successful! Cookies captured.";
                result.CookieCount = cookies.Count;
                
                _logger.LogInformation("Captured {CookieCount} cookies for user: {Username}", 
                    cookies.Count, username);

                // Notify client of success
                if (_notificationService != null && !string.IsNullOrEmpty(connectionId))
                {
                    await _notificationService.NotifyAuthStatusAsync(
                        connectionId, 
                        $"Success! Captured {cookies.Count} cookies.");
                }
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Timeout waiting for authentication completion. QR code may not have been scanned.");
                result.Success = false;
                result.Message = "Authentication timeout. Please scan the QR code within 90 seconds.";
                
                // Take screenshot for debugging
                await page.ScreenshotAsync(new() 
                { 
                    Path = $"ibkr-auth-timeout-{DateTime.Now:yyyyMMddHHmmss}.png" 
                });

                // Notify client of timeout
                if (_notificationService != null && !string.IsNullOrEmpty(connectionId))
                {
                    await _notificationService.NotifyAuthStatusAsync(
                        connectionId, 
                        "Timeout: QR code was not scanned within 90 seconds.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication");
                result.Success = false;
                result.Message = $"Authentication error: {ex.Message}";

                // Notify client of error
                if (_notificationService != null && !string.IsNullOrEmpty(connectionId))
                {
                    await _notificationService.NotifyAuthStatusAsync(
                        connectionId, 
                        $"Error: {ex.Message}");
                }
            }
            finally
            {
                // Cleanup
                await context.CloseAsync();
                await browser.CloseAsync();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in automated authentication");
            return new IbkrAuthenticationResult
            {
                Success = false,
                Username = username,
                Message = $"Fatal error: {ex.Message}"
            };
        }
    }
}

public class IbkrAuthenticationResult
{
    public bool Success { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int CookieCount { get; set; }
}

/// <summary>
/// Service for sending real-time notifications during IBKR authentication
/// </summary>
public interface IIbkrAuthNotificationService
{
    Task SendQRCodeImageAsync(string connectionId, string base64Image);
    Task NotifyAuthStatusAsync(string connectionId, string message);
}
