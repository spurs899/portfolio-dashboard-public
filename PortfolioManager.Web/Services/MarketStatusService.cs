using System.Net.Http.Json;
using PortfolioManager.Contracts.Models.Market;
using PortfolioManager.Contracts.Web;

namespace PortfolioManager.Web.Services;

public interface IMarketStatusService
{
    Task<MarketStatusInfo> GetMarketStatusAsync();
}

public class MarketStatusService : IMarketStatusService
{
    private readonly HttpClient _httpClient;
    private readonly bool _demoMode;

    public MarketStatusService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _demoMode = configuration.GetValue<bool>("DemoMode");
    }

    public async Task<MarketStatusInfo> GetMarketStatusAsync()
    {
        if (_demoMode)
        {
            return GetCalculatedMarketStatus();
        }

        try
        {
            var response = await _httpClient.GetFromJsonAsync<MarketStatusResponse>("api/Market/status");
            
            if (response == null)
            {
                return GetCalculatedMarketStatus();
            }

            return new MarketStatusInfo
            {
                IsOpen = response.IsOpen,
                Status = MapStatus(response.Market),
                Source = response.Source,
                NyseTime = DateTime.TryParse(response.ServerTime, out var time) 
                    ? time.ToString("h:mm:ss tt") 
                    : DateTime.UtcNow.AddHours(-5).ToString("h:mm:ss tt")
            };
        }
        catch
        {
            return GetCalculatedMarketStatus();
        }
    }

    private MarketStatusInfo GetCalculatedMarketStatus()
    {
        var utcNow = DateTime.UtcNow;
        var isDST = IsDaylightSavingTime(utcNow);
        var easternOffset = isDST ? -4 : -5;
        var nyseNow = utcNow.AddHours(easternOffset);

        string status;
        bool isOpen;

        if (nyseNow.DayOfWeek == DayOfWeek.Saturday || nyseNow.DayOfWeek == DayOfWeek.Sunday)
        {
            status = "Market Closed (Weekend)";
            isOpen = false;
        }
        else
        {
            var marketOpen = new TimeSpan(9, 30, 0);
            var marketClose = new TimeSpan(16, 0, 0);
            var currentTime = nyseNow.TimeOfDay;

            if (currentTime >= marketOpen && currentTime < marketClose)
            {
                status = "Market Open";
                isOpen = true;
            }
            else if (currentTime < marketOpen)
            {
                status = "Pre-Market";
                isOpen = false;
            }
            else
            {
                status = "After Hours";
                isOpen = false;
            }
        }

        return new MarketStatusInfo
        {
            IsOpen = isOpen,
            Status = status,
            Source = "calculated",
            NyseTime = nyseNow.ToString("h:mm:ss tt")
        };
    }

    private string MapStatus(string apiStatus)
    {
        return apiStatus switch
        {
            "open" => "Market Open",
            "closed" => "Market Closed",
            "early_hours" => "Pre-Market",
            _ => "Market Closed"
        };
    }

    private bool IsDaylightSavingTime(DateTime utcTime)
    {
        var year = utcTime.Year;

        var marchFirst = new DateTime(year, 3, 1);
        var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)marchFirst.DayOfWeek + 7) % 7;
        var dstStart = marchFirst.AddDays(daysUntilSunday + 7).AddHours(7);

        var novemberFirst = new DateTime(year, 11, 1);
        daysUntilSunday = ((int)DayOfWeek.Sunday - (int)novemberFirst.DayOfWeek + 7) % 7;
        var dstEnd = novemberFirst.AddDays(daysUntilSunday).AddHours(6);

        return utcTime >= dstStart && utcTime < dstEnd;
    }
}