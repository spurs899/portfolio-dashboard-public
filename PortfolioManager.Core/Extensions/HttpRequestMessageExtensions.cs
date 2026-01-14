using System.Net.Http.Headers;

namespace PortfolioManager.Core.Extensions;

public static class HttpRequestMessageExtensions
{
    public static void AddCommonBrowserHeaders(this HttpRequestMessage request)
    {
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
    }

    public static void AddSharesiesBrowserHeaders(this HttpRequestMessage request, string origin)
    {
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Referer", origin);
        request.Headers.Add("sec-ch-ua", "\"Google Chrome\";v=\"143\", \"Chromium\";v=\"143\", \"Not A(Brand)\";v=\"24\"");
        request.Headers.Add("sec-ch-ua-mobile", "?0");
        request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
        request.Headers.Add("sec-fetch-dest", "empty");
        request.Headers.Add("sec-fetch-mode", "cors");
        request.Headers.Add("sec-fetch-site", "cross-site");
    }

    public static void AddBearerToken(this HttpRequestMessage request, string? token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
