using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using PortfolioManager.Contracts.Models.Brokerage;
using PortfolioManager.Contracts.Models.Shared;
using PortfolioManager.Contracts.Web;
using PortfolioManager.Web.Services;
using PortfolioManager.Web.Helpers;
using AggregatedHoldingsTable = PortfolioManager.Web.Components.ViewModels.AggregatedHoldingsTable;
using DetailedHoldingsTable = PortfolioManager.Web.Components.ViewModels.DetailedHoldingsTable;

namespace PortfolioManager.Web.Pages;

public partial class Home : IDisposable
{
    private const string DefaultCurrency = "N/A";
    
    [Inject] private ISharesiesService SharesiesService { get; set; } = default!;
    [Inject] private IAuthStateService AuthStateService { get; set; } = default!;
    [Inject] private IIbkrService IbkrService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    [Inject] private IMarketStatusService MarketStatusService { get; set; } = default!;
    [Inject] private ICurrencyService CurrencyService { get; set; } = default!;
    
    private bool _demoMode;
    private bool _isLoggedIn;
    private bool _isLoading;
    private string? _userId;
    private string? _ibkrUsername; // Track IBKR login separately
    private PortfolioResponse? _portfolioData;
    
    // Cached computed values
    private decimal _cachedTotalValue;
    private decimal _cachedDailyReturn;
    private decimal _cachedDailyReturnPercentage;
    
    // Mobile expanded holdings tracking
    private readonly HashSet<string> _expandedMobileHoldings = [];
    
    // Desktop expanded holdings tracking
    private readonly HashSet<string> _expandedDesktopHoldings = [];
    
    // Time and timezone tracking
    private Timer? _timer;
    private string _localTime = "";
    private string _localTimeZone = "";
    private string _nyseTime = "";
    private string _marketStatus = "";

    protected override async Task OnInitializedAsync()
    {
        _demoMode = Configuration.GetValue<bool>("DemoMode");
        await CurrencyService.InitializeAsync();
        CurrencyService.OnCurrencyChanged += OnCurrencyChanged;
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
        
        if (nyseNow.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
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
        CurrencyService.OnCurrencyChanged -= OnCurrencyChanged;
    }
    
    private void OnCurrencyChanged()
    {
        StateHasChanged();
    }

    private async Task TryAutoLoginAsync()
    {
        // Try to restore Sharesies session
        var isAuthenticated = await AuthStateService.IsAuthenticatedAsync();
        if (isAuthenticated)
        {
            _userId = await AuthStateService.GetUserIdAsync();
            _isLoggedIn = true;
        }
        
        // Try to restore IBKR session
        var storedIbkrUsername = await IbkrService.GetStoredUsernameAsync();
        if (!string.IsNullOrEmpty(storedIbkrUsername))
        {
            // Validate if IBKR session is still valid by trying to fetch accounts
            var isIbkrSessionValid = await IbkrService.ValidateSessionAsync();
            if (isIbkrSessionValid)
            {
                _ibkrUsername = storedIbkrUsername;
            }
            else
            {
                // Session expired, clear it
                await IbkrService.ClearSessionAsync();
            }
        }
        
        // Load portfolio data if either authentication is valid
        if (_isLoggedIn || !string.IsNullOrEmpty(_ibkrUsername))
        {
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
        var dialog = await DialogService.ShowAsync<SharesiesLoginDialog>("Connect to Sharesies", parameters, options);
    }

    private async Task ShowIbkrLoginDialog()
    {
        var parameters = new DialogParameters
        {
            { "OnLoginSuccess", EventCallback.Factory.Create<string>(this, OnIbkrLoginSuccess) }
        };
        
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<IbkrLoginDialog>("Connect to Interactive Brokers", parameters, options);
    }

    private async Task OnLoginSuccess(string userId)
    {
        _userId = userId;
        _isLoggedIn = true;
        await LoadPortfolioData();
        StateHasChanged();
    }

    private async Task OnIbkrLoginSuccess(string username)
    {
        _ibkrUsername = username;
        Snackbar.Add($"Successfully connected to IBKR as {username}!", Severity.Success);
        
        // Load IBKR portfolio data
        await LoadPortfolioData();
        StateHasChanged();
    }

    private async Task LoadPortfolioData()
    {
        try
        {
            _isLoading = true;
            StateHasChanged();

            // Load Sharesies data if logged in
            PortfolioResponse? sharesiesData = null;
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
                
                sharesiesData = await SharesiesService.GetPortfolioAsync(authResult);
                
                if (sharesiesData == null)
                {
                    await HandleAuthenticationExpired();
                    return;
                }
            }

            // Load IBKR data if logged in
            PortfolioResponse? ibkrData = null;
            if (!string.IsNullOrEmpty(_ibkrUsername))
            {
                ibkrData = await LoadIbkrPortfolioData();
            }

            // Aggregate data from both brokerages
            _portfolioData = AggregatePortfolioData(sharesiesData, ibkrData);
            
            if (_portfolioData != null)
            {
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

    private async Task<PortfolioResponse?> LoadIbkrPortfolioData()
    {
        try
        {
            // Get IBKR accounts
            var accounts = await IbkrService.GetAccountsAsync();
            if (accounts?.Accounts == null || !accounts.Accounts.Any())
            {
                // Session likely expired or invalid
                await IbkrService.ClearSessionAsync();
                _ibkrUsername = null;
                Snackbar.Add("IBKR session expired. Please reconnect.", Severity.Warning);
                return null;
            }

            var firstAccount = accounts.Accounts.First();
            
            // Get positions
            var positions = await IbkrService.GetPositionsAsync(firstAccount.Id!);
            if (positions == null || !positions.Any())
            {
                return new PortfolioResponse
                {
                    UserProfile = new UserProfileDto
                    {
                        Name = "IBKR User",
                        BrokerageType = (int)BrokerageType.InteractiveBrokers
                    },
                    Instruments = []
                };
            }

            return new PortfolioResponse
            {
                UserProfile = new UserProfileDto
                {
                    Name = $"IBKR - {_ibkrUsername ?? "User"}",
                    BrokerageType = (int)BrokerageType.InteractiveBrokers
                },
                Instruments = positions
            };
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
        {
            // IBKR service is unavailable or session expired
            await IbkrService.ClearSessionAsync();
            _ibkrUsername = null;
            Snackbar.Add("IBKR service is currently unavailable or session expired. Please reconnect.", Severity.Error);
            return null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                                               ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            // Session expired or invalid
            await IbkrService.ClearSessionAsync();
            _ibkrUsername = null;
            Snackbar.Add("IBKR session expired. Please reconnect.", Severity.Warning);
            return null;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading IBKR portfolio: {ex.Message}", Severity.Warning);
            return null;
        }
    }

    private PortfolioResponse? AggregatePortfolioData(PortfolioResponse? sharesies, PortfolioResponse? ibkr)
    {
        // If only one source has data, return it
        if (sharesies == null && ibkr == null) return null;
        if (sharesies == null) return ibkr;
        if (ibkr == null) return sharesies;

        // Combine instruments from both sources
        var allInstruments = new List<InstrumentDto>();
        
        if (sharesies.Instruments != null)
            allInstruments.AddRange(sharesies.Instruments);
        
        if (ibkr.Instruments != null)
            allInstruments.AddRange(ibkr.Instruments);

        return new PortfolioResponse
        {
            UserProfile = sharesies.UserProfile ?? ibkr.UserProfile,
            Instruments = allInstruments
        };
    }

    private async Task HandleAuthenticationExpired()
    {
        await AuthStateService.ClearAuthStateAsync();
        await IbkrService.ClearSessionAsync(); // Also clear IBKR session
        _isLoggedIn = false;
        _userId = null;
        _ibkrUsername = null;
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
    
    private async Task SetCurrencyAsync(string currency)
    {
        await CurrencyService.SetCurrencyAsync(currency);
    }

    private decimal GetTotalValue() => ConvertCurrency(_cachedTotalValue, GetPortfolioCurrency());
    private decimal GetDailyReturn() => ConvertCurrency(_cachedDailyReturn, GetPortfolioCurrency());
    private decimal GetDailyReturnPercentage() => _cachedDailyReturnPercentage;
    
    private decimal ConvertCurrency(decimal amount, string sourceCurrency)
    {
        if (string.IsNullOrEmpty(sourceCurrency) || sourceCurrency == DefaultCurrency)
            return amount;
        return CurrencyService.ConvertToSelectedCurrency(amount, sourceCurrency);
    }
    
    private string FormatCurrency(decimal amount, string sourceCurrency)
    {
        return CurrencyService.FormatCurrency(amount, sourceCurrency);
    }

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
                var currency = firstInstrument.Currency ?? DefaultCurrency;
                
                var totalShares = instruments.Sum(i => i.SharesOwned);
                var totalValue = instruments.Sum(i => i.InvestmentValue);
                var totalCostBasis = instruments.Sum(i => i.CostBasis);
                var totalReturn = totalValue - totalCostBasis;
                var averageReturnPercentage = totalCostBasis > 0 ? (totalReturn / totalCostBasis * 100) : 0;
                
                return new AggregatedHoldingsTable.AggregatedHoldingViewModel
                {
                    Symbol = firstInstrument.Symbol ?? "N/A",
                    Name = firstInstrument.Name ?? "",
                    Currency = currency,
                    SharePrice = ConvertCurrency(firstInstrument.SharePrice, currency),
                    TotalShares = totalShares,
                    TotalValue = ConvertCurrency(totalValue, currency),
                    TotalReturn = ConvertCurrency(totalReturn, currency),
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
                var currency = instrument.Currency ?? DefaultCurrency;
                var dailyReturnPercent = instrument.InvestmentValue > 0 
                    ? (instrument.SimpleReturn / instrument.InvestmentValue * 100) 
                    : 0;

                return new DetailedHoldingsTable.HoldingViewModel
                {
                    Symbol = instrument.Symbol ?? "N/A",
                    Name = instrument.Name ?? "",
                    Currency = currency,
                    SharePrice = ConvertCurrency(instrument.SharePrice, currency),
                    SharesOwned = instrument.SharesOwned,
                    InvestmentValue = ConvertCurrency(instrument.InvestmentValue, currency),
                    DailyReturn = ConvertCurrency(instrument.SimpleReturn, currency),
                    DailyReturnPercentage = dailyReturnPercent,
                    BrokerageType = instrument.BrokerageType
                };
            })
            .OrderByDescending(x => x.InvestmentValue)
            .ToList();
    }

    private string GetSymbolAvatarText(string symbol) => BrokerageHelpers.GetSymbolAvatarText(symbol);

    private string GetBrokerageIcon(int brokerageType) => BrokerageHelpers.GetBrokerageIcon(brokerageType);

    private string GetBrokerageName(int brokerageType) => BrokerageHelpers.GetBrokerageName(brokerageType);

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

    private void HandleExpandKeyDown(KeyboardEventArgs e, string symbol)
    {
        if (e.Key == "Enter" || e.Key == " ")
        {
            ToggleDesktopHolding(symbol);
        }
    }
}
