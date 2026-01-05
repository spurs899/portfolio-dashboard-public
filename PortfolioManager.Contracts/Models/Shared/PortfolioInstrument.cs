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
}