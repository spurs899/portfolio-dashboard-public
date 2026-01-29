namespace PortfolioManager.Contracts.Models.Shared;

public class PortfolioInstrument
{
    public string Id { get; set; }
    
    public string Symbol { get; set; }
    public string Name { get; set; }
    public string Currency { get; set; }
    
    
    public BrokerageType BrokerageType { get; set; }
    
    public decimal SharesOwned { get; set; }
    public decimal SharePrice { get; set; }
    
    // Additional fields from Sharesies
    public decimal InvestmentValue { get; set; }
    public decimal CostBasis { get; set; }
    public decimal TotalReturn { get; set; }
    public decimal SimpleReturn { get; set; }
    public decimal DividendsReceived { get; set; }
}