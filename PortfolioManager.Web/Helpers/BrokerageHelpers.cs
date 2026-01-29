namespace PortfolioManager.Web.Helpers;

public static class BrokerageHelpers
{
    public static string GetBrokerageName(int brokerageType) => brokerageType switch
    {
        0 => "Manual",
        1 => "Sharesies",
        2 => "Hatch",
        3 => "Stake",
        4 => "ASB Securities",
        5 => "ANZ Securities",
        6 => "Invest Direct",
        7 => "Interactive Brokers",
        8 => "Other",
        _ => "Unknown"
    };
    
    public static int GetBrokerageTypeFromName(string brokerageName) => brokerageName switch
    {
        "Manual" => 8, // Map Manual to Other for backward compatibility
        "Sharesies" => 1,
        "Hatch" => 2,
        "Stake" => 3,
        "ASB Securities" => 4,
        "ANZ Securities" => 5,
        "Invest Direct" or "Jarden Direct" => 6,
        "Interactive Brokers" or "IBKR" => 7,
        "Other" => 8,
        _ => 8 // Default to Other instead of Manual
    };
    
    public static string GetBrokerageIcon(int brokerageType) => brokerageType switch
    {
        0 => "images/generic-brokerage.svg",
        1 => "images/sharesies-logo.png",
        2 => "images/hatch-logo.png",
        3 => "images/stake-logo.png",
        4 => "images/asb-logo.jpeg",
        5 => "images/anz-logo.jpeg",
        6 => "images/invest-direct.png",
        7 => "images/ibkr-logo.svg",
        8 => "images/generic-brokerage.svg",
        _ => "images/generic-brokerage.svg"
    };
    
    public static string GetSymbolAvatarText(string symbol)
    {
        if (string.IsNullOrEmpty(symbol) || symbol == "N/A")
            return "?";
        
        return symbol.Length >= 2 ? symbol[..2].ToUpper() : symbol.ToUpper();
    }
}
