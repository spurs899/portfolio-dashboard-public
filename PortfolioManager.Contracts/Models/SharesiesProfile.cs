using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PortfolioManager.Contracts.Models;

public class SharesiesProfileResponse
{
    [JsonPropertyName("profiles")]
    public List<SharesiesProfile>? Profiles { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class SharesiesProfile
{
    [JsonPropertyName("active_sign_up_id")]
    public string? ActiveSignUpId { get; set; }

    [JsonPropertyName("avatar_colour")]
    public string? AvatarColour { get; set; }

    [JsonPropertyName("avatar_initials")]
    public string? AvatarInitials { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("is_in_signup")]
    public bool IsInSignup { get; set; }

    [JsonPropertyName("is_personal")]
    public bool IsPersonal { get; set; }

    [JsonPropertyName("legacy_customer_id")]
    public string? LegacyCustomerId { get; set; }

    [JsonPropertyName("legacy_profile_type")]
    public string? LegacyProfileType { get; set; }

    [JsonPropertyName("multi_actor")]
    public bool MultiActor { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("owner_id")]
    public string? OwnerId { get; set; }

    [JsonPropertyName("portfolios")]
    public List<SharesiesProfilePortfolio>? Portfolios { get; set; }

    [JsonPropertyName("should_redirect_to_invest")]
    public bool ShouldRedirectToInvest { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("supports_native_overview")]
    public bool SupportsNativeOverview { get; set; }

    [JsonPropertyName("verification_status")]
    public string? VerificationStatus { get; set; }
}

public class SharesiesProfilePortfolio
{
    [JsonPropertyName("balance")]
    public string? Balance { get; set; }

    [JsonPropertyName("balance_is_estimate")]
    public bool BalanceIsEstimate { get; set; }

    [JsonPropertyName("estimated_balance")]
    public string? EstimatedBalance { get; set; }

    [JsonPropertyName("holding_balance")]
    public string? HoldingBalance { get; set; }

    [JsonPropertyName("holding_balance_is_estimate")]
    public bool HoldingBalanceIsEstimate { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("permissions")]
    public List<string>? Permissions { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("include_sold_investments")]
    public bool? IncludeSoldInvestments { get; set; }
}