using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PortfolioManager.Contracts.Models;

public class SharesiesLoginResponse
{
    [JsonPropertyName("application_pointers")]
    public List<object>? ApplicationPointers { get; set; }

    [JsonPropertyName("authenticated")]
    public bool Authenticated { get; set; }

    [JsonPropertyName("can_enter_address_token")]
    public bool CanEnterAddressToken { get; set; }

    [JsonPropertyName("can_join_kiwisaver")]
    public bool CanJoinKiwisaver { get; set; }

    [JsonPropertyName("distill_token")]
    public string? DistillToken { get; set; }

    [JsonPropertyName("distill_token_v2")]
    public string? DistillTokenV2 { get; set; }

    [JsonPropertyName("employment_fund_ids")]
    public List<object>? EmploymentFundIds { get; set; }

    [JsonPropertyName("flags")]
    public List<string>? Flags { get; set; }

    [JsonPropertyName("ga_id")]
    public string? GaId { get; set; }

    [JsonPropertyName("include_sold_investments")]
    public bool IncludeSoldInvestments { get; set; }

    [JsonPropertyName("is_eligible_for_kiwisaver")]
    public bool IsEligibleForKiwisaver { get; set; }

    [JsonPropertyName("live_data")]
    public SharesiesLiveData? LiveData { get; set; }

    [JsonPropertyName("on_kiwisaver_us_waitlist")]
    public bool OnKiwisaverUsWaitlist { get; set; }

    [JsonPropertyName("on_kiwisaver_waitlist")]
    public bool OnKiwisaverWaitlist { get; set; }

    [JsonPropertyName("orders")]
    public List<object>? Orders { get; set; }

    [JsonPropertyName("participants")]
    public List<string>? Participants { get; set; }

    [JsonPropertyName("portfolio")]
    public List<SharesiesPortfolioHolding>? Portfolio { get; set; }

    [JsonPropertyName("portfolio_enable_extended_hours")]
    public bool PortfolioEnableExtendedHours { get; set; }

    [JsonPropertyName("portfolio_filter_preference")]
    public string? PortfolioFilterPreference { get; set; }

    [JsonPropertyName("portfolio_sort_preference")]
    public string? PortfolioSortPreference { get; set; }

    [JsonPropertyName("preferred_product")]
    public object? PreferredProduct { get; set; }

    [JsonPropertyName("rakaia_token")]
    public string? RakaiaToken { get; set; }

    [JsonPropertyName("rakaia_token_expiry")]
    public SharesiesQuantumContainer? RakaiaTokenExpiry { get; set; }

    [JsonPropertyName("referral_code")]
    public string? ReferralCode { get; set; }

    [JsonPropertyName("registered_preference_at")]
    public object? RegisteredPreferenceAt { get; set; }

    [JsonPropertyName("save_accounts")]
    public Dictionary<string, object>? SaveAccounts { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("user")]
    public SharesiesUser? User { get; set; }

    [JsonPropertyName("user_list")]
    public List<SharesiesUserListItem>? UserList { get; set; }
}

public class SharesiesQuantumContainer
{
    [JsonPropertyName("$quantum")]
    public long? Quantum { get; set; }
}

public class SharesiesUserListItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("preferred_name")]
    public string? PreferredName { get; set; }

    [JsonPropertyName("primary")]
    public bool Primary { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }
}

public class SharesiesLiveData
{
    [JsonPropertyName("eligible_for_free_month")]
    public bool EligibleForFreeMonth { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }
}

public class SharesiesPortfolioHolding
{
    [JsonPropertyName("fund_id")]
    public string? FundId { get; set; }

    [JsonPropertyName("holding_type")]
    public string? HoldingType { get; set; }

    [JsonPropertyName("shares")]
    public string? Shares { get; set; }

    [JsonPropertyName("shares_active")]
    public string? SharesActive { get; set; }
}