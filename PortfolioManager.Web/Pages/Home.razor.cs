using Microsoft.AspNetCore.Components;
using MudBlazor;
using PortfolioManager.Contracts.Models.Brokerage;
using PortfolioManager.Contracts.Web;
using PortfolioManager.Web.Services;
using AggregatedHoldingsTable = PortfolioManager.Web.Components.ViewModels.AggregatedHoldingsTable;
using DetailedHoldingsTable = PortfolioManager.Web.Components.ViewModels.DetailedHoldingsTable;

namespace PortfolioManager.Web.Pages;

public partial class Home : IDisposable
{
    private const string DefaultCurrency = "N/A";
    
    [Inject] private IBrokerageService BrokerageService { get; set; } = default!;
    [Inject] private IAuthStateService AuthStateService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    [Inject] private IMarketStatusService MarketStatusService { get; set; } = default!;
    
    private bool _demoMode;
    private bool _isLoggedIn;
    private bool _isLoading;
    private string? _userId;
    private PortfolioResponse? _portfolioData;
    
    // Cached computed values
    private decimal _cachedTotalValue;
    private decimal _cachedDailyReturn;
    private decimal _cachedDailyReturnPercentage;
    
    // Mobile expanded holdings tracking
    private HashSet<string> _expandedMobileHoldings = new();
    
    // Desktop expanded holdings tracking
    private HashSet<string> _expandedDesktopHoldings = new();
    
    // Time and timezone tracking
    private Timer? _timer;
    private string _localTime = "";
    private string _localTimeZone = "";
    private string _nyseTime = "";
    private string _marketStatus = "";

    protected override async Task OnInitializedAsync()
    {
        _demoMode = Configuration.GetValue<bool>("DemoMode");
        UpdateTimes();
        StartTimerAsync();
        await TryAutoLoginAsync();
        
        if (!_demoMode)
        {
            await UpdateMarketStatusFromApi();
        }
    }

    private void StartTimerAsync()
    {
        _timer = new Timer(async void (_) =>
        {
            UpdateTimes();
            
            if (!_demoMode && DateTime.Now.Second == 0)
            {
                await UpdateMarketStatusFromApi();
            }
            
            await InvokeAsync(StateHasChanged);
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    private async Task UpdateMarketStatusFromApi()
    {
        try
        {
            var status = await MarketStatusService.GetMarketStatusAsync();
            _marketStatus = status.Status;
        }
        catch
        {
            UpdateTimes();
        }
    }

    private void UpdateTimes()
    {
        var now = DateTime.Now;
        var utcNow = DateTime.UtcNow;
        
        var isDST = IsDaylightSavingTime(utcNow);
        var easternOffset = isDST ? -4 : -5;
        var nyseNow = utcNow.AddHours(easternOffset);
        
        _localTime = now.ToString("h:mm:ss tt");
        _localTimeZone = $"UTC{GetUtcOffset(now)}";
        _nyseTime = nyseNow.ToString("h:mm:ss tt");
        
        if (nyseNow.DayOfWeek == DayOfWeek.Saturday || nyseNow.DayOfWeek == DayOfWeek.Sunday)
        {
            _marketStatus = "Market Closed (Weekend)";
        }
        else
        {
            var marketOpen = new TimeSpan(9, 30, 0);
            var marketClose = new TimeSpan(16, 0, 0);
            var currentTime = nyseNow.TimeOfDay;
            
            if (currentTime >= marketOpen && currentTime < marketClose)
            {
                _marketStatus = "Market Open";
            }
            else if (currentTime < marketOpen)
            {
                _marketStatus = "Pre-Market";
            }
            else
            {
                _marketStatus = "After Hours";
            }
        }
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

    private string GetUtcOffset(DateTime localTime)
    {
        var offset = TimeZoneInfo.Local.GetUtcOffset(localTime);
        var hours = offset.Hours;
        var minutes = Math.Abs(offset.Minutes);
        
        if (hours >= 0)
        {
            return minutes > 0 ? $"+{hours}:{minutes:D2}" : $"+{hours}";
        }
        else
        {
            return minutes > 0 ? $"{hours}:{minutes:D2}" : $"{hours}";
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private async Task TryAutoLoginAsync()
    {
        var isAuthenticated = await AuthStateService.IsAuthenticatedAsync();
        if (isAuthenticated)
        {
            _userId = await AuthStateService.GetUserIdAsync();
            _isLoggedIn = true;
            await LoadPortfolioData();
        }
    }

    private async Task ShowLoginDialog()
    {
        var parameters = new DialogParameters
        {
            { "OnLoginSuccess", EventCallback.Factory.Create<string>(this, OnLoginSuccess) }
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<LoginDialog>("Login to Sharesies", parameters, options);
    }

    private async Task OnLoginSuccess(string userId)
    {
        _userId = userId;
        _isLoggedIn = true;
        await LoadPortfolioData();
        StateHasChanged();
    }

    private async Task LoadPortfolioData()
    {
        try
        {
            _isLoading = true;
            StateHasChanged();

            if (!string.IsNullOrEmpty(_userId))
            {
                var (_, userId, tokens) = await AuthStateService.GetAuthDataAsync();
                
                if (tokens == null || tokens.Count == 0)
                {
                    await HandleAuthenticationExpired();
                    return;
                }
                
                var authResult = new AuthenticationResult
                {
                    IsAuthenticated = true,
                    UserId = userId ?? _userId,
                    Tokens = tokens,
                    Step = AuthenticationStep.Completed
                };
                
                _portfolioData = await BrokerageService.GetPortfolioAsync(authResult);
                
                if (_portfolioData == null)
                {
                    await HandleAuthenticationExpired();
                    return;
                }
                
                ComputePortfolioMetrics();
            }

            _isLoading = false;
            StateHasChanged();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await HandleAuthenticationExpired();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading portfolio: {ex.Message}", Severity.Error);
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task HandleAuthenticationExpired()
    {
        await AuthStateService.ClearAuthStateAsync();
        _isLoggedIn = false;
        _userId = null;
        _portfolioData = null;
        _isLoading = false;
        
        Snackbar.Add("Session expired. Please login again.", Severity.Warning);
        StateHasChanged();
    }
    
    private void ComputePortfolioMetrics()
    {
        if (_portfolioData?.Instruments == null)
        {
            _cachedTotalValue = 0;
            _cachedDailyReturn = 0;
            _cachedDailyReturnPercentage = 0;
            return;
        }

        _cachedTotalValue = 0;
        _cachedDailyReturn = 0;

        foreach (var instrument in _portfolioData.Instruments)
        {
            _cachedTotalValue += instrument.InvestmentValue;
            _cachedDailyReturn += instrument.SimpleReturn;
        }

        _cachedDailyReturnPercentage = _cachedTotalValue > 0
            ? (_cachedDailyReturn / _cachedTotalValue * 100) 
            : 0;
    }

    private async Task RefreshPortfolio()
    {
        await LoadPortfolioData();
        Snackbar.Add("Portfolio refreshed", Severity.Success);
    }

    private decimal GetTotalValue() => _cachedTotalValue;
    private decimal GetDailyReturn() => _cachedDailyReturn;
    private decimal GetDailyReturnPercentage() => _cachedDailyReturnPercentage;

    private int GetHoldingsCount()
    {
        return _portfolioData?.Instruments?.Select(i => i.Symbol).Distinct().Count() ?? 0;
    }

    private string GetPortfolioCurrency()
    {
        var firstInstrument = _portfolioData?.Instruments?.FirstOrDefault();
        return firstInstrument?.Currency ?? DefaultCurrency;
    }

    private List<AggregatedHoldingsTable.AggregatedHoldingViewModel> GetAggregatedHoldings()
    {
        if (_portfolioData?.Instruments == null)
            return new List<AggregatedHoldingsTable.AggregatedHoldingViewModel>();

        return _portfolioData.Instruments
            .GroupBy(i => i.Symbol)
            .Select(group =>
            {
                var instruments = group.ToList();
                var firstInstrument = instruments.First();
                
                var totalShares = instruments.Sum(i => i.SharesOwned);
                var totalValue = instruments.Sum(i => i.InvestmentValue);
                var totalCostBasis = instruments.Sum(i => i.CostBasis);
                var totalReturn = totalValue - totalCostBasis;
                var averageReturnPercentage = totalCostBasis > 0 ? (totalReturn / totalCostBasis * 100) : 0;
                
                return new AggregatedHoldingsTable.AggregatedHoldingViewModel
                {
                    Symbol = firstInstrument.Symbol ?? "N/A",
                    Name = firstInstrument.Name ?? "",
                    Currency = firstInstrument.Currency ?? DefaultCurrency,
                    SharePrice = firstInstrument.SharePrice,
                    TotalShares = totalShares,
                    TotalValue = totalValue,
                    TotalReturn = totalReturn,
                    AverageReturnPercentage = averageReturnPercentage,
                    BrokerageTypes = instruments.Select(i => i.BrokerageType).Distinct().OrderBy(x => x).ToList()
                };
            })
            .OrderByDescending(x => x.TotalValue)
            .ToList();
    }

    private List<DetailedHoldingsTable.HoldingViewModel> GetHoldings()
    {
        if (_portfolioData?.Instruments == null)
            return [];

        return _portfolioData.Instruments
            .Select(instrument =>
            {
                var dailyReturnPercent = instrument.InvestmentValue > 0 
                    ? (instrument.SimpleReturn / instrument.InvestmentValue * 100) 
                    : 0;

                return new DetailedHoldingsTable.HoldingViewModel
                {
                    Symbol = instrument.Symbol ?? "N/A",
                    Name = instrument.Name ?? "",
                    Currency = instrument.Currency ?? DefaultCurrency,
                    SharePrice = instrument.SharePrice,
                    SharesOwned = instrument.SharesOwned,
                    InvestmentValue = instrument.InvestmentValue,
                    DailyReturn = instrument.SimpleReturn,
                    DailyReturnPercentage = dailyReturnPercent,
                    BrokerageType = instrument.BrokerageType
                };
            })
            .OrderByDescending(x => x.InvestmentValue)
            .ToList();
    }

    private string GetSymbolAvatarText(string symbol)
    {
        if (string.IsNullOrEmpty(symbol) || symbol == "N/A")
            return "?";
        
        return symbol.Length >= 2 ? symbol[..2].ToUpper() : symbol.ToUpper();
    }

    private string GetBrokerageIcon(int brokerageType)
    {
        return brokerageType switch
        {
            0 => "images/sharesies-logo.png",
            1 => "images/ibkr-logo.svg",
            _ => ""
        };
    }

    private string GetBrokerageName(int brokerageType)
    {
        return brokerageType switch
        {
            0 => "Sharesies",
            1 => "Interactive Brokers",
            _ => "Unknown"
        };
    }

    private void ToggleMobileHolding(string symbol)
    {
        if (_expandedMobileHoldings.Contains(symbol))
        {
            _expandedMobileHoldings.Remove(symbol);
        }
        else
        {
            _expandedMobileHoldings.Add(symbol);
        }
    }

    private void ToggleDesktopHolding(string symbol)
    {
        if (_expandedDesktopHoldings.Contains(symbol))
        {
            _expandedDesktopHoldings.Remove(symbol);
        }
        else
        {
            _expandedDesktopHoldings.Add(symbol);
        }
    }
}
