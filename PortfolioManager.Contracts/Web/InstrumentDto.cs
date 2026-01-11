using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Web;

public class InstrumentDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("brokerageType")]
    public int BrokerageType { get; set; }

    [JsonPropertyName("sharesOwned")]
    public decimal SharesOwned { get; set; }

    [JsonPropertyName("sharePrice")]
    public decimal SharePrice { get; set; }

    [JsonPropertyName("investmentValue")]
    public decimal InvestmentValue { get; set; }

    [JsonPropertyName("costBasis")]
    public decimal CostBasis { get; set; }

    [JsonPropertyName("totalReturn")]
    public decimal TotalReturn { get; set; }

    [JsonPropertyName("simpleReturn")]
    public decimal SimpleReturn { get; set; }

    [JsonPropertyName("dividendsReceived")]
    public decimal DividendsReceived { get; set; }
}