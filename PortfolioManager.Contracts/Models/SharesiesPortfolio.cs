using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PortfolioManager.Contracts.Models;

public class SharesiesPortfolio
{
    [JsonPropertyName("instrument_returns")]
    public Dictionary<string, SharesiesInstrumentReturn>? InstrumentReturns { get; set; }
}

public class SharesiesInstrumentReturn
{
    [JsonPropertyName("instrument_uuid")]
    public string? InstrumentUuid { get; set; }

    [JsonPropertyName("shares_owned")]
    public decimal SharesOwned { get; set; }

    [JsonPropertyName("investment_value")]
    public decimal InvestmentValue { get; set; }

    [JsonPropertyName("investment_value_home")]
    public decimal InvestmentValueHome { get; set; }

    [JsonPropertyName("dividends_received")]
    public decimal DividendsReceived { get; set; }

    [JsonPropertyName("dividends_received_home")]
    public decimal DividendsReceivedHome { get; set; }

    [JsonPropertyName("simple_return")]
    public decimal SimpleReturn { get; set; }

    [JsonPropertyName("total_return")]
    public decimal TotalReturn { get; set; }

    [JsonPropertyName("total_return_home")]
    public decimal TotalReturnHome { get; set; }

    [JsonPropertyName("cost_basis")]
    public decimal CostBasis { get; set; }

    [JsonPropertyName("cost_basis_max")]
    public decimal CostBasisMax { get; set; }

    [JsonPropertyName("total_cost_basis")]
    public decimal TotalCostBasis { get; set; }

    [JsonPropertyName("transaction_fees")]
    public decimal TransactionFees { get; set; }

    [JsonPropertyName("tax_paid")]
    public decimal TaxPaid { get; set; }

    [JsonPropertyName("managed_fund_transaction_fees")]
    public decimal ManagedFundTransactionFees { get; set; }

    [JsonPropertyName("shares_detail")]
    public SharesiesSharesDetail? SharesDetail { get; set; }

    [JsonPropertyName("total_return_detail")]
    public SharesiesTotalReturnDetail? TotalReturnDetail { get; set; }

    [JsonPropertyName("amount_put_in_detail")]
    public SharesiesAmountPutInDetail? AmountPutInDetail { get; set; }

    [JsonPropertyName("unvested_detail")]
    public SharesiesUnvestedDetail? UnvestedDetail { get; set; }

    [JsonPropertyName("unrealised_tax_paid")]
    public decimal UnrealisedTaxPaid { get; set; }

    [JsonPropertyName("unrealised_dividends")]
    public decimal UnrealisedDividends { get; set; }

    [JsonPropertyName("unrealised_simple_return")]
    public decimal UnrealisedSimpleReturn { get; set; }

    [JsonPropertyName("unrealised_total_return")]
    public decimal UnrealisedTotalReturn { get; set; }
}

public class SharesiesSharesDetail
{
    [JsonPropertyName("shares_bought")]
    public decimal SharesBought { get; set; }

    [JsonPropertyName("shares_sold")]
    public decimal SharesSold { get; set; }

    [JsonPropertyName("shares_transferred_in")]
    public decimal SharesTransferredIn { get; set; }

    [JsonPropertyName("shares_transferred_out")]
    public decimal SharesTransferredOut { get; set; }
}

public class SharesiesTotalReturnDetail
{
    [JsonPropertyName("dividends")]
    public decimal Dividends { get; set; }

    [JsonPropertyName("managed_fund_transaction_fees")]
    public decimal ManagedFundTransactionFees { get; set; }

    [JsonPropertyName("realised_capital_gains")]
    public decimal RealisedCapitalGains { get; set; }

    [JsonPropertyName("transaction_fees")]
    public decimal TransactionFees { get; set; }

    [JsonPropertyName("adr_fees")]
    public decimal AdrFees { get; set; }

    [JsonPropertyName("unrealised_adr_fees")]
    public decimal UnrealisedAdrFees { get; set; }

    [JsonPropertyName("unrealised_capital_gains")]
    public decimal UnrealisedCapitalGains { get; set; }

    [JsonPropertyName("unrealised_capital_gains_home")]
    public decimal UnrealisedCapitalGainsHome { get; set; }

    [JsonPropertyName("unrealised_dividends")]
    public decimal UnrealisedDividends { get; set; }

    [JsonPropertyName("unrealised_tax_paid")]
    public decimal UnrealisedTaxPaid { get; set; }
}

public class SharesiesAmountPutInDetail
{
    [JsonPropertyName("transaction_fees_buys")]
    public decimal TransactionFeesBuys { get; set; }

    [JsonPropertyName("unrealised_managed_fund_transaction_fees")]
    public decimal UnrealisedManagedFundTransactionFees { get; set; }
}

public class SharesiesUnvestedDetail
{
    [JsonPropertyName("shares_unvested")]
    public decimal SharesUnvested { get; set; }

    [JsonPropertyName("unvested_capital_gains")]
    public decimal UnvestedCapitalGains { get; set; }

    [JsonPropertyName("unvested_investment_value")]
    public decimal UnvestedInvestmentValue { get; set; }

    [JsonPropertyName("unvested_simple_return")]
    public decimal UnvestedSimpleReturn { get; set; }
}
