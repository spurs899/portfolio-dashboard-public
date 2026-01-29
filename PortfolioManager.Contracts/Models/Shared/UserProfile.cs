namespace PortfolioManager.Contracts.Models.Shared;

public class UserProfile
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public BrokerageType BrokerageType { get; set; }
}