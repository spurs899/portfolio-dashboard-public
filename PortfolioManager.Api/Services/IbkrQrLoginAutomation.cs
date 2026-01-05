using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace PortfolioManager.Api.Services;

public class IbkrQrLoginAutomation
{
    public class Result
    {
        public string SessionId { get; set; } = string.Empty;
        public byte[] QrImage { get; set; } = Array.Empty<byte>();
        public object? SessionCookies { get; set; }
        public bool Authenticated { get; set; }
    }

    public async Task<Result> StartLoginAsync(string username, string password)
    {
        var result = new Result { SessionId = Guid.NewGuid().ToString() };
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
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
        var qrImageBytes = await qrElement.ScreenshotAsync();
        result.QrImage = qrImageBytes;

        // Wait for authentication (user scans QR and approves in app)
        // This is a simple polling loop; in production, use a more robust approach
        for (int i = 0; i < 60; i++) // up to 60s
        {
            if (await page.Locator("text=Welcome" ).IsVisibleAsync() || await page.Locator(".account-summary").IsVisibleAsync())
            {
                result.Authenticated = true;
                break;
            }
            await Task.Delay(1000);
        }

        // Extract session cookies if authenticated
        if (result.Authenticated)
        {
            var cookies = await context.CookiesAsync();
            result.SessionCookies = cookies;
        }

        await browser.CloseAsync();
        return result;
    }
}
