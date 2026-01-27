namespace PortfolioManager.Web.Services;

public interface ICurrencyService
{
    string SelectedCurrency { get; }
    event Action? OnCurrencyChanged;
    
    Task InitializeAsync();
    Task SetCurrencyAsync(string currency);
    decimal ConvertToSelectedCurrency(decimal amount, string sourceCurrency);
    string FormatCurrency(decimal amount, string? sourceCurrency = null);
}
