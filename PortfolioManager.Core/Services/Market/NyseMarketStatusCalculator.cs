using PortfolioManager.Contracts.Models.Market;

namespace PortfolioManager.Core.Services.Market;

public class NyseMarketStatusCalculator : IMarketStatusCalculator
{
    public MarketStatusResponse CalculateMarketStatus()
    {
        var utcNow = DateTime.UtcNow;
        var isDST = IsDaylightSavingTime(utcNow);
        var easternOffset = isDST ? -4 : -5;
        var nyseNow = utcNow.AddHours(easternOffset);

        string status;
        bool isOpen;

        if (nyseNow.DayOfWeek == DayOfWeek.Saturday || nyseNow.DayOfWeek == DayOfWeek.Sunday)
        {
            status = "closed";
            isOpen = false;
        }
        else if (IsNyseHoliday(nyseNow))
        {
            status = "closed";
            isOpen = false;
        }
        else
        {
            var marketOpen = new TimeSpan(9, 30, 0);
            var marketClose = new TimeSpan(16, 0, 0);
            var currentTime = nyseNow.TimeOfDay;

            if (currentTime >= marketOpen && currentTime < marketClose)
            {
                status = "open";
                isOpen = true;
            }
            else if (currentTime < marketOpen)
            {
                status = "early_hours";
                isOpen = false;
            }
            else
            {
                status = "closed";
                isOpen = false;
            }
        }

        return new MarketStatusResponse
        {
            Market = status,
            ServerTime = $"{nyseNow:yyyy-MM-ddTHH:mm:ss}{(isDST ? "-04:00" : "-05:00")}",
            NyseStatus = status,
            NasdaqStatus = status,
            IsOpen = isOpen,
            Source = "calculated"
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

    private bool IsNyseHoliday(DateTime date)
    {
        var year = date.Year;
        var holidays = GetNyseHolidays(year);
        return holidays.Any(h => h.Date == date.Date);
    }

    private List<DateTime> GetNyseHolidays(int year)
    {
        var holidays = new List<DateTime>();

        var newYears = new DateTime(year, 1, 1);
        holidays.Add(AdjustForWeekend(newYears));

        holidays.Add(GetNthWeekdayOfMonth(year, 1, DayOfWeek.Monday, 3));
        holidays.Add(GetNthWeekdayOfMonth(year, 2, DayOfWeek.Monday, 3));
        holidays.Add(GetGoodFriday(year));
        holidays.Add(GetLastWeekdayOfMonth(year, 5, DayOfWeek.Monday));

        var juneteenth = new DateTime(year, 6, 19);
        holidays.Add(AdjustForWeekend(juneteenth));

        var independenceDay = new DateTime(year, 7, 4);
        holidays.Add(AdjustForWeekend(independenceDay));

        holidays.Add(GetNthWeekdayOfMonth(year, 9, DayOfWeek.Monday, 1));
        holidays.Add(GetNthWeekdayOfMonth(year, 11, DayOfWeek.Thursday, 4));

        var christmas = new DateTime(year, 12, 25);
        holidays.Add(AdjustForWeekend(christmas));

        return holidays;
    }

    private DateTime AdjustForWeekend(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday)
            return date.AddDays(-1);
        if (date.DayOfWeek == DayOfWeek.Sunday)
            return date.AddDays(1);
        return date;
    }

    private DateTime GetNthWeekdayOfMonth(int year, int month, DayOfWeek dayOfWeek, int n)
    {
        var firstDay = new DateTime(year, month, 1);
        var daysUntilTarget = ((int)dayOfWeek - (int)firstDay.DayOfWeek + 7) % 7;
        return firstDay.AddDays(daysUntilTarget + (n - 1) * 7);
    }

    private DateTime GetLastWeekdayOfMonth(int year, int month, DayOfWeek dayOfWeek)
    {
        var lastDay = new DateTime(year, month, DateTime.DaysInMonth(year, month));
        var daysBack = ((int)lastDay.DayOfWeek - (int)dayOfWeek + 7) % 7;
        return lastDay.AddDays(-daysBack);
    }

    private DateTime GetGoodFriday(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var month = (h + l - 7 * m + 114) / 31;
        var day = ((h + l - 7 * m + 114) % 31) + 1;

        var easter = new DateTime(year, month, day);
        return easter.AddDays(-2);
    }
}
