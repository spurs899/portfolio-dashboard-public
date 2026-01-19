namespace PortfolioManager.Contracts.Models;

// Authentication Response Models
public class IbkrAuthResponse
{
    public bool Authenticated { get; set; }
    public bool Competing { get; set; }
    public bool Connected { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, object>? UserInfo { get; set; }
}

// SSO Validation
public class IbkrSsoValidateResponse
{
    public bool Valid { get; set; }
    public string? UserId { get; set; }
    public string? User { get; set; }
}

// Accounts Response
public class IbkrAccountsResponse
{
    public List<IbkrAccount>? Accounts { get; set; }
    public List<string>? Aliases { get; set; }
    public bool? AllowFeatures { get; set; }
    public string? SelectedAccount { get; set; }
}

public class IbkrAccount
{
    public string? Id { get; set; }
    public string? AccountId { get; set; }
    public string? AccountVan { get; set; }
    public string? AccountTitle { get; set; }
    public string? DisplayName { get; set; }
    public string? AccountAlias { get; set; }
    public string? AccountStatus { get; set; }
    public string? Currency { get; set; }
    public string? Type { get; set; }
    public bool? TradingType { get; set; }
}

// Portfolio Summary
public class IbkrPortfolioSummary
{
    public Dictionary<string, IbkrSummaryItem>? Commodities { get; set; }
    public Dictionary<string, IbkrSummaryItem>? Securities { get; set; }
    public Dictionary<string, IbkrSummaryItem>? Total { get; set; }
}

public class IbkrSummaryItem
{
    public decimal? Amount { get; set; }
    public decimal? Quantity { get; set; }
    public string? Currency { get; set; }
    public bool? IsNull { get; set; }
    public string? Severity { get; set; }
}

// Positions
public class IbkrPositionsResponse
{
    public List<IbkrPosition>? Positions { get; set; }
}

public class IbkrPosition
{
    public string? AcctId { get; set; }
    public int? ConId { get; set; }
    public string? ContractDesc { get; set; }
    public string? AssetClass { get; set; }
    public decimal? Position { get; set; }
    public decimal? MktPrice { get; set; }
    public decimal? MktValue { get; set; }
    public string? Currency { get; set; }
    public decimal? AvgCost { get; set; }
    public decimal? AvgPrice { get; set; }
    public decimal? RealizedPnl { get; set; }
    public decimal? UnrealizedPnl { get; set; }
    public string? Exchs { get; set; }
    public string? Expiry { get; set; }
    public string? PutOrCall { get; set; }
    public string? Multiplier { get; set; }
    public decimal? Strike { get; set; }
    public string? Ticker { get; set; }
    public string? UndConId { get; set; }
    public string? Model { get; set; }
    public string? Time { get; set; }
    public decimal? Change { get; set; }
    public decimal? PercentChange { get; set; }
}

// Security Definition
public class IbkrSecDefRequest
{
    public List<int>? Conids { get; set; }
}

public class IbkrSecDefResponse
{
    public List<IbkrSecurityDefinition>? Secdef { get; set; }
}

public class IbkrSecurityDefinition
{
    public int? ConId { get; set; }
    public string? Symbol { get; set; }
    public string? SecType { get; set; }
    public string? ListingExchange { get; set; }
    public string? CompanyName { get; set; }
    public string? Currency { get; set; }
}
