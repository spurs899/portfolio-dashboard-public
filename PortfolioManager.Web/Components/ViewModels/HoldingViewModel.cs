namespace PortfolioManager.Web.Components.ViewModels;

public class DetailedHoldingsTable
{
    public class HoldingViewModel
    {
        public string Symbol { get; set; } = "";
        public string Name { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal SharePrice { get; set; }
        public decimal SharesOwned { get; set; }
        public decimal InvestmentValue { get; set; }
        public decimal DailyReturn { get; set; }
        public decimal DailyReturnPercentage { get; set; }
        public int BrokerageType { get; set; }
    }
}