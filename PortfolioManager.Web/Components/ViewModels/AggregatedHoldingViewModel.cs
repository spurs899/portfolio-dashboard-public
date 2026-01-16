namespace PortfolioManager.Web.Components.ViewModels;

public class AggregatedHoldingsTable
{
    public class AggregatedHoldingViewModel
    {
        public string Symbol { get; set; } = "";
        public string Name { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal SharePrice { get; set; }
        public decimal TotalShares { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalReturn { get; set; }
        public decimal AverageReturnPercentage { get; set; }
        public List<int> BrokerageTypes { get; set; } = new();
    }
}