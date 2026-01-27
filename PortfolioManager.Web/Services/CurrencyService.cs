using Blazored.LocalStorage;
using System.Net.Http.Json;

namespace PortfolioManager.Web.Services;

public class CurrencyService : ICurrencyService
{
    private const string CurrencyStorageKey = "selected_currency";
    private const string DefaultCurrency = "USD";
    private const string FrankfurterApiUrl = "https://api.frankfurter.app/latest?from=USD";
    
    private readonly ILocalStorageService _localStorage;
    private string _selectedCurrency = DefaultCurrency;
    
    // Fallback exchange rates if API fails
    private readonly Dictionary<string, decimal> _exchangeRates = new()
    {
        { "USD", 1.0m },
        { "NZD", 1.65m }
    };

    public string SelectedCurrency => _selectedCurrency;
    public event Action? OnCurrencyChanged;

    public CurrencyService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        // Fetch live exchange rates
        await UpdateExchangeRatesAsync();
        
        // Restore saved currency preference
        var savedCurrency = await _localStorage.GetItemAsStringAsync(CurrencyStorageKey);
        if (!string.IsNullOrEmpty(savedCurrency))
        {
            savedCurrency = savedCurrency.Trim('"');
            if (_exchangeRates.ContainsKey(savedCurrency))
            {
                _selectedCurrency = savedCurrency;
            }
        }
    }

    private async Task UpdateExchangeRatesAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetFromJsonAsync<FrankfurterResponse>(FrankfurterApiUrl);
            
            if (response?.Rates != null)
            {
                // Update rates from API (keep USD at 1.0)
                foreach (var rate in response.Rates)
                {
                    _exchangeRates[rate.Key] = rate.Value;
                }
            }
        }
        catch
        {
            // Silently fall back to hardcoded rates if API fails
        }
    }

    public async Task SetCurrencyAsync(string currency)
    {
        if (!_exchangeRates.ContainsKey(currency))
        {
            throw new ArgumentException($"Unsupported currency: {currency}");
        }

        _selectedCurrency = currency;
        await _localStorage.SetItemAsStringAsync(CurrencyStorageKey, currency);
        OnCurrencyChanged?.Invoke();
    }

    public decimal ConvertToSelectedCurrency(decimal amount, string sourceCurrency)
    {
        if (!_exchangeRates.ContainsKey(sourceCurrency))
        {
            return amount;
        }

        if (sourceCurrency == _selectedCurrency)
        {
            return amount;
        }

        // Convert to USD first, then to target currency
        var amountInUsd = sourceCurrency == "USD" ? amount : amount / _exchangeRates[sourceCurrency];
        var convertedAmount = amountInUsd * _exchangeRates[_selectedCurrency];
        
        return convertedAmount;
    }

    public string FormatCurrency(decimal amount, string? sourceCurrency = null)
    {
        var displayAmount = string.IsNullOrEmpty(sourceCurrency) 
            ? amount 
            : ConvertToSelectedCurrency(amount, sourceCurrency);

        return _selectedCurrency switch
        {
            "USD" => $"${displayAmount:N2}",
            "NZD" => $"${displayAmount:N2} NZD",
            _ => $"{displayAmount:N2}"
        };
    }
    
    private record FrankfurterResponse(string Base, Dictionary<string, decimal> Rates);
}
