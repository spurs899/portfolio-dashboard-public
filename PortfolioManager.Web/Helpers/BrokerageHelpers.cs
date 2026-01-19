namespace PortfolioManager.Web.Helpers;

public static class BrokerageHelpers
{
    public static string GetBrokerageName(int brokerageType) => brokerageType switch
    {
        0 => "Sharesies",
        1 => "Interactive Brokers",
        _ => "Unknown"
    };
    
    public static string GetBrokerageIcon(int brokerageType) => brokerageType switch
    {
        0 => "images/sharesies-logo.png",
        1 => "images/ibkr-logo.svg",
        _ => ""
    };
    
    public static string GetSymbolAvatarText(string symbol)
    {
        if (string.IsNullOrEmpty(symbol) || symbol == "N/A")
            return "?";
        
        return symbol.Length >= 2 ? symbol[..2].ToUpper() : symbol.ToUpper();
    }
}
