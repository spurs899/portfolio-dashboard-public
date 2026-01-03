using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PortfolioManager.Contracts.Models;

public class SharesiesInstrumentResponse
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("filesHostAddress")]
    public string? FilesHostAddress { get; set; }

    [JsonPropertyName("categories")]
    public List<string>? Categories { get; set; }

    [JsonPropertyName("exchanges")]
    public List<SharesiesExchange>? Exchanges { get; set; }

    [JsonPropertyName("assetManagers")]
    public List<SharesiesAssetManager>? AssetManagers { get; set; }

    [JsonPropertyName("instrumentTypes")]
    public List<SharesiesInstrumentType>? InstrumentTypes { get; set; }

    [JsonPropertyName("filterOptions")]
    public List<string>? FilterOptions { get; set; }

    [JsonPropertyName("sortOptions")]
    public List<SharesiesSortOption>? SortOptions { get; set; }

    [JsonPropertyName("priceChangeTimeOptions")]
    public List<string>? PriceChangeTimeOptions { get; set; }
}

public class SharesiesExchange
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("timeZone")]
    public string? TimeZone { get; set; }

    [JsonPropertyName("openHours")]
    public List<SharesiesOpenHour>? OpenHours { get; set; }
}

public class SharesiesOpenHour
{
    [JsonPropertyName("start")]
    public string? Start { get; set; }

    [JsonPropertyName("finish")]
    public string? Finish { get; set; }
}

public class SharesiesAssetManager
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("websiteUrl")]
    public string? WebsiteUrl { get; set; }

    [JsonPropertyName("shortName")]
    public string? ShortName { get; set; }

    [JsonPropertyName("timeZone")]
    public string? TimeZone { get; set; }

    [JsonPropertyName("externalId")]
    public string? ExternalId { get; set; }
}

public class SharesiesInstrumentType
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class SharesiesSortOption
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class SharesiesInstrument
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("urlSlug")]
    public string? UrlSlug { get; set; }

    [JsonPropertyName("isClientReadOnly")]
    public bool IsClientReadOnly { get; set; }

    [JsonPropertyName("instrumentType")]
    public string? InstrumentType { get; set; }

    [JsonPropertyName("isAdr")]
    public bool IsAdr { get; set; }

    [JsonPropertyName("isUsPartnership")]
    public bool IsUsPartnership { get; set; }

    [JsonPropertyName("isIlliquid")]
    public bool IsIlliquid { get; set; }

    [JsonPropertyName("isFif")]
    public bool IsFif { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("kidsRecommended")]
    public bool KidsRecommended { get; set; }

    [JsonPropertyName("isVolatile")]
    public bool IsVolatile { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("drivewealthId")]
    public string? DrivewealthId { get; set; }

    [JsonPropertyName("krakenId")]
    public string? KrakenId { get; set; }

    [JsonPropertyName("categories")]
    public List<string>? Categories { get; set; }

    [JsonPropertyName("logoIdentifier")]
    public string? LogoIdentifier { get; set; }

    [JsonPropertyName("logos")]
    public SharesiesInstrumentLogos? Logos { get; set; }

    [JsonPropertyName("riskRating")]
    public int RiskRating { get; set; }

    [JsonPropertyName("timeHorizonMin")]
    public object? TimeHorizonMin { get; set; }

    [JsonPropertyName("comparisonPrices")]
    public Dictionary<string, SharesiesComparisonPrice?>? ComparisonPrices { get; set; }

    [JsonPropertyName("marketPrice")]
    public string? MarketPrice { get; set; }

    [JsonPropertyName("marketLastCheck")]
    public string? MarketLastCheck { get; set; }

    [JsonPropertyName("extendedHoursPrice")]
    public string? ExtendedHoursPrice { get; set; }

    [JsonPropertyName("extendedHoursLastCheck")]
    public string? ExtendedHoursLastCheck { get; set; }

    [JsonPropertyName("tradingStatus")]
    public string? TradingStatus { get; set; }

    [JsonPropertyName("extendedHoursTradingStatus")]
    public string? ExtendedHoursTradingStatus { get; set; }

    [JsonPropertyName("extendedHoursNotionalStatus")]
    public string? ExtendedHoursNotionalStatus { get; set; }

    [JsonPropertyName("exchangeCountry")]
    public string? ExchangeCountry { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("peRatio")]
    public string? PeRatio { get; set; }

    [JsonPropertyName("marketCap")]
    public long MarketCap { get; set; }

    [JsonPropertyName("parentInstrumentId")]
    public string? ParentInstrumentId { get; set; }

    [JsonPropertyName("fmsFundId")]
    public string? FmsFundId { get; set; }

    [JsonPropertyName("underlyingInstrumentId")]
    public string? UnderlyingInstrumentId { get; set; }

    [JsonPropertyName("underlyingInstrument")]
    public object? UnderlyingInstrument { get; set; }

    [JsonPropertyName("sharesPerRight")]
    public decimal? SharesPerRight { get; set; }

    [JsonPropertyName("offerPrice")]
    public string? OfferPrice { get; set; }

    [JsonPropertyName("exercisePrice")]
    public string? ExercisePrice { get; set; }

    [JsonPropertyName("websiteUrl")]
    public string? WebsiteUrl { get; set; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; set; }

    [JsonPropertyName("legacyImageUrl")]
    public string? LegacyImageUrl { get; set; }

    [JsonPropertyName("dominantColour")]
    public string? DominantColour { get; set; }

    [JsonPropertyName("pdsDriveId")]
    public string? PdsDriveId { get; set; }

    [JsonPropertyName("assetManager")]
    public string? AssetManager { get; set; }

    [JsonPropertyName("managementType")]
    public string? ManagementType { get; set; }

    [JsonPropertyName("fixedFeeSpread")]
    public string? FixedFeeSpread { get; set; }

    [JsonPropertyName("managementFeePercent")]
    public string? ManagementFeePercent { get; set; }

    [JsonPropertyName("grossDividendYieldPercent")]
    public string? GrossDividendYieldPercent { get; set; }

    [JsonPropertyName("annualisedReturnPercent")]
    public string? AnnualisedReturnPercent { get; set; }

    [JsonPropertyName("firstTradingDateTimeUtc")]
    public string? FirstTradingDateTimeUtc { get; set; }

    [JsonPropertyName("lastTradingDateTimeUtc")]
    public string? LastTradingDateTimeUtc { get; set; }

    [JsonPropertyName("ceo")]
    public string? Ceo { get; set; }

    [JsonPropertyName("employees")]
    public int Employees { get; set; }

    [JsonPropertyName("etfHoldings")]
    public object? EtfHoldings { get; set; }

    [JsonPropertyName("assetClasses")]
    public object? AssetClasses { get; set; }

    [JsonPropertyName("fundRiskCategory")]
    public object? FundRiskCategory { get; set; }

    [JsonPropertyName("incorporationCountry")]
    public string? IncorporationCountry { get; set; }

    [JsonPropertyName("ric")]
    public string? Ric { get; set; }

    [JsonPropertyName("tradingWarning")]
    public string? TradingWarning { get; set; }

    [JsonPropertyName("orderUnitsMinimum")]
    public string? OrderUnitsMinimum { get; set; }

    [JsonPropertyName("orderUnitsMinimumIncrement")]
    public string? OrderUnitsMinimumIncrement { get; set; }

    [JsonPropertyName("orderPriceIncrement")]
    public string? OrderPriceIncrement { get; set; }

    [JsonPropertyName("orderCostIncrement")]
    public string? OrderCostIncrement { get; set; }

    [JsonPropertyName("settlementOffset")]
    public string? SettlementOffset { get; set; }
}

public class SharesiesInstrumentLogos
{
    [JsonPropertyName("wide")]
    public string? Wide { get; set; }

    [JsonPropertyName("thumb")]
    public string? Thumb { get; set; }

    [JsonPropertyName("micro")]
    public string? Micro { get; set; }
}

public class SharesiesComparisonPrice
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("percent")]
    public string? Percent { get; set; }

    [JsonPropertyName("max")]
    public string? Max { get; set; }

    [JsonPropertyName("min")]
    public string? Min { get; set; }
}