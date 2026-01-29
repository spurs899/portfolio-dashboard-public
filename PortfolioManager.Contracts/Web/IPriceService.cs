namespace PortfolioManager.Contracts.Web;

public interface IPriceService
{
    Task<Dictionary<string, PriceData>> GetPricesAsync(IEnumerable<string> symbols);
    Task<PriceData?> GetPriceAsync(string symbol);
}

public class PriceData
{
    public decimal CurrentPrice { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePercent { get; set; }
    public decimal PreviousClose { get; set; }
}
