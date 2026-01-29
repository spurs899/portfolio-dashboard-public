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
    
    [Inject] private IHoldingsStorageService HoldingsStorage { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private ICurrencyService CurrencyService { get; set; } = default!;
    [Inject] private IPriceService PriceService { get; set; } = default!;
    [Inject] private Microsoft.Extensions.Configuration.IConfiguration Configuration { get; set; } = default!;
    
    private bool _isLoading;
    private bool _isDemoMode;
    private PortfolioResponse? _portfolioData;
    private List<Holding> _holdings = new();
    
    // Cached computed values
    private decimal _cachedTotalValue;
    private decimal _cachedDailyReturn;
    private decimal _cachedDailyReturnPercentage;
    private bool _hasDailyReturnError; // Track if daily return fetch failed
    
    // Unified holdings tracking for Robinhood UI
    private readonly HashSet<string> _expandedHoldings = [];
    
    // Time and timezone tracking
    private Timer? _timer;
    private string _localTime = "";
    private string _localTimeZone = "";
    private string _nyseTime = "";
    private string _marketStatus = "";

    protected override async Task OnInitializedAsync()
    {
        _isDemoMode = Configuration.GetValue<bool>("DemoMode");
        
        await CurrencyService.InitializeAsync();
        CurrencyService.OnCurrencyChanged += OnCurrencyChanged;
        UpdateTimes();
        StartTimerAsync();
        
        // Load holdings from local storage
        await LoadPortfolioData();
    }

    private void StartTimerAsync()
    {
        _timer = new Timer(async void (_) =>
        {
            UpdateTimes();
            await InvokeAsync(StateHasChanged);
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
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

    private async Task LoadPortfolioData()
    {
        try
        {
            _isLoading = true;
            StateHasChanged();

            // Load holdings from local storage
            _holdings = await HoldingsStorage.GetHoldingsAsync();
            
            if (_holdings.Count == 0)
            {
                // No holdings - show empty state
                _portfolioData = null;
            }
            else
            {
                // Convert holdings to portfolio format with mock prices
                _portfolioData = await ConvertHoldingsToPortfolio(_holdings);
                
                if (_portfolioData != null)
                {
                    ComputePortfolioMetrics();
                }
            }

            _isLoading = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading portfolio: {ex.Message}", Severity.Error);
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task<PortfolioResponse> ConvertHoldingsToPortfolio(List<Holding> holdings)
    {
        // Get distinct symbols
        var symbols = holdings.Select(h => h.Symbol).Distinct().ToList();
        
        // Fetch real prices from Finnhub
        Console.WriteLine($"[Home] Fetching prices for {symbols.Count} symbols...");
        var prices = await PriceService.GetPricesAsync(symbols);
        Console.WriteLine($"[Home] Received {prices.Count} prices");
        
        // Group by brokerage
        var instrumentsList = new List<InstrumentDto>();
        bool anyPriceFetchFailed = false;
        
        foreach (var holding in holdings)
        {
            // Get real current price and daily change - no fallbacks
            decimal currentPrice;
            decimal dailyReturn = 0;
            
            if (prices.TryGetValue(holding.Symbol, out var priceData))
            {
                currentPrice = priceData.CurrentPrice;
                // Calculate daily return from actual price change
                dailyReturn = priceData.Change * holding.Shares;
                
                Console.WriteLine($"[Home] Using real price for {holding.Symbol}: ${currentPrice:F2}, Daily change: ${priceData.Change:F2}");
            }
            else
            {
                // No fallback - use 0 to indicate missing data
                currentPrice = 0;
                dailyReturn = 0;
                anyPriceFetchFailed = true;
                
                Console.WriteLine($"[Home] Unable to fetch price for {holding.Symbol}");
            }
            
            var marketValue = currentPrice * holding.Shares;
            // Only calculate cost basis if user provided one
            var costBasis = holding.AverageCost > 0 ? holding.AverageCost * holding.Shares : 0m;
            var simpleReturn = marketValue - costBasis;
            
            var instrument = new InstrumentDto
            {
                Id = holding.Symbol,
                Symbol = holding.Symbol,
                Name = holding.Name,
                SharesOwned = holding.Shares,
                InvestmentValue = marketValue,
                SimpleReturn = dailyReturn, // Using actual daily return from Finnhub
                TotalReturn = simpleReturn,
                SharePrice = currentPrice,
                Currency = "USD",
                CostBasis = costBasis, // Add cost basis to DTO
                BrokerageType = MapBrokerageType(holding.BrokerageType)
            };
            
            instrumentsList.Add(instrument);
        }
        
        // Track if any price fetch failed for UI display
        _hasDailyReturnError = anyPriceFetchFailed;
        
        return new PortfolioResponse
        {
            UserProfile = new UserProfileDto
            {
                Name = "Portfolio Dashboard",
                BrokerageType = 0
            },
            Instruments = instrumentsList
        };
    }
    
    private int MapBrokerageType(string brokerageType)
    {
        return BrokerageHelpers.GetBrokerageTypeFromName(brokerageType);
    }
    
    // Holdings Management Methods
    private async Task ShowAddHoldingDialog()
    {
        var options = new DialogOptions 
        { 
            CloseOnEscapeKey = true, 
            MaxWidth = MaxWidth.Small, 
            FullWidth = true
        };
        
        var parameters = new DialogParameters();
        
        var dialog = await DialogService.ShowAsync<AddEditHoldingDialog>("Add Holding", parameters, options);
        var result = await dialog.Result;
        
        if (!result.Canceled && result.Data is Holding newHolding)
        {
            try
            {
                var wasMerged = await HoldingsStorage.AddHoldingAsync(newHolding);
                await LoadPortfolioData();
                
                if (wasMerged)
                {
                    Snackbar.Add($"Merged {newHolding.Symbol} with existing holding at {newHolding.BrokerageType}", Severity.Success);
                }
                else
                {
                    Snackbar.Add($"Added {newHolding.Symbol} to your portfolio", Severity.Success);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error adding holding: {ex.Message}", Severity.Error);
            }
        }
    }
    
    private async Task ShowEditHoldingDialog(string symbol)
    {
        var holding = _holdings.FirstOrDefault(h => h.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        if (holding == null)
        {
            return;
        }
        
        var parameters = new DialogParameters
        {
            { "ExistingHolding", holding }
        };
        
        var options = new DialogOptions 
        { 
            CloseOnEscapeKey = true, 
            MaxWidth = MaxWidth.Small, 
            FullWidth = true
        };
        
        var dialog = await DialogService.ShowAsync<AddEditHoldingDialog>("Edit Holding", parameters, options);
        var result = await dialog.Result;
        
        if (!result.Canceled && result.Data is Holding updatedHolding)
        {
            try
            {
                await HoldingsStorage.UpdateHoldingAsync(symbol, updatedHolding);
                await LoadPortfolioData();
                Snackbar.Add($"Updated {symbol}", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error updating holding: {ex.Message}", Severity.Error);
            }
        }
    }

    private async Task ShowEditBrokerHoldingDialog(string symbol, int brokerageType)
    {
        var brokerageName = GetBrokerageName(brokerageType);
        var holding = _holdings.FirstOrDefault(h => 
            h.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase) && 
            h.BrokerageType.Equals(brokerageName, StringComparison.OrdinalIgnoreCase));
        if (holding == null)
        {
            return;
        }
        
        var parameters = new DialogParameters
        {
            { "ExistingHolding", holding }
        };
        
        var options = new DialogOptions 
        { 
            CloseOnEscapeKey = true, 
            MaxWidth = MaxWidth.Small, 
            FullWidth = true
        };
        
        var dialog = await DialogService.ShowAsync<AddEditHoldingDialog>("Edit Holding", parameters, options);
        var result = await dialog.Result;
        
        if (!result.Canceled && result.Data is Holding updatedHolding)
        {
            try
            {
                await HoldingsStorage.UpdateHoldingAsync(symbol, updatedHolding);
                await LoadPortfolioData();
                Snackbar.Add($"Updated {symbol} at {brokerageName}", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error updating holding: {ex.Message}", Severity.Error);
            }
        }
    }
    
    private async Task DeleteHolding(string symbol)
    {
        var confirmed = await DialogService.ShowMessageBox(
            "Delete Holding",
            $"Are you sure you want to delete {symbol} from your portfolio?",
            yesText: "Delete",
            cancelText: "Cancel"
        );
        
        if (confirmed == true)
        {
            try
            {
                await HoldingsStorage.DeleteHoldingAsync(symbol);
                await LoadPortfolioData();
                Snackbar.Add($"Deleted {symbol}", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error deleting holding: {ex.Message}", Severity.Error);
            }
        }
    }

    private async Task DeleteBrokerHolding(string symbol, int brokerageType)
    {
        var brokerageName = GetBrokerageName(brokerageType);
        var confirmed = await DialogService.ShowMessageBox(
            "Delete Holding",
            $"Are you sure you want to delete {symbol} from {brokerageName}?",
            yesText: "Delete",
            cancelText: "Cancel"
        );
        
        if (confirmed == true)
        {
            try
            {
                await HoldingsStorage.DeleteHoldingAsync(symbol, brokerageName);
                await LoadPortfolioData();
                Snackbar.Add($"Deleted {symbol} from {brokerageName}", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error deleting holding: {ex.Message}", Severity.Error);
            }
        }
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
    
    private async Task SetCurrencyAsync(string currency)
    {
        await CurrencyService.SetCurrencyAsync(currency);
        
        var currencyName = currency switch
        {
            "USD" => "US Dollars (USD)",
            "NZD" => "New Zealand Dollars (NZD)",
            _ => currency
        };
        
        Snackbar.Add($"Now showing prices in {currencyName}", Severity.Info);
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
                    HasCostBasis = totalCostBasis > 0,
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
    
    private void ToggleHolding(string symbol)
    {
        if (_expandedHoldings.Contains(symbol))
        {
            _expandedHoldings.Remove(symbol);
        }
        else
        {
            _expandedHoldings.Add(symbol);
        }
    }
    
    private async Task RefreshData()
    {
        Console.WriteLine("[Home] Manual refresh triggered - fetching fresh data...");
        
        try
        {
            _isLoading = true;
            StateHasChanged();
            
            // Refresh exchange rates
            Console.WriteLine("[Home] Refreshing currency exchange rates...");
            await CurrencyService.RefreshExchangeRatesAsync();
            
            // Refresh stock prices
            Console.WriteLine("[Home] Refreshing stock prices...");
            await LoadPortfolioData();
            
            Snackbar.Add("Portfolio refreshed with latest data", Severity.Success);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Home] Error refreshing: {ex.Message}");
            Snackbar.Add("Error refreshing portfolio", Severity.Error);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
}
