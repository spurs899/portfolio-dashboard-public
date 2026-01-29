using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PortfolioManager.Contracts.Models;

public class SharesiesUser
{
    [JsonPropertyName("account_frozen")]
    public bool AccountFrozen { get; set; }

    [JsonPropertyName("account_reference")]
    public string? AccountReference { get; set; }

    [JsonPropertyName("account_restricted")]
    public bool AccountRestricted { get; set; }

    [JsonPropertyName("account_restricted_date")]
    public string? AccountRestrictedDate { get; set; }

    [JsonPropertyName("active_sign_up_flow")]
    public string? ActiveSignUpFlow { get; set; }

    [JsonPropertyName("address")]
    public SharesiesAddress? Address { get; set; }

    [JsonPropertyName("address_reject_reason")]
    public string? AddressRejectReason { get; set; }

    [JsonPropertyName("address_state")]
    public string? AddressState { get; set; }

    [JsonPropertyName("calendar_tax_year")]
    public int CalendarTaxYear { get; set; }

    [JsonPropertyName("checks")]
    public SharesiesUserChecks? Checks { get; set; }

    [JsonPropertyName("created")]
    public SharesiesQuantumContainer? Created { get; set; }

    [JsonPropertyName("customer_signup_state")]
    public string? CustomerSignupState { get; set; }

    [JsonPropertyName("date_of_birth")]
    public string? DateOfBirth { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }

    [JsonPropertyName("first_tax_year")]
    public int FirstTaxYear { get; set; }

    [JsonPropertyName("has_current_email_verification_token")]
    public bool HasCurrentEmailVerificationToken { get; set; }

    [JsonPropertyName("has_seen")]
    public SharesiesHasSeen? HasSeen { get; set; }

    [JsonPropertyName("holding_balance")]
    public string? HoldingBalance { get; set; }

    [JsonPropertyName("home_currency")]
    public string? HomeCurrency { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("id_type")]
    public string? IdType { get; set; }

    [JsonPropertyName("ird_number")]
    public string? IrdNumber { get; set; }

    [JsonPropertyName("is_dependent")]
    public bool IsDependent { get; set; }

    [JsonPropertyName("is_owner_prescribed")]
    public bool IsOwnerPrescribed { get; set; }

    [JsonPropertyName("jurisdiction")]
    public string? Jurisdiction { get; set; }

    [JsonPropertyName("kiwisaver_customer_state")]
    public string? KiwisaverCustomerState { get; set; }

    [JsonPropertyName("mfa_enabled")]
    public bool MfaEnabled { get; set; }

    [JsonPropertyName("other_prescribed_participant")]
    public string? OtherPrescribedParticipant { get; set; }

    [JsonPropertyName("participant_emails")]
    public List<string>? ParticipantEmails { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("pir")]
    public string? Pir { get; set; }

    [JsonPropertyName("portfolio_id")]
    public string? PortfolioId { get; set; }

    [JsonPropertyName("portfolio_intro_cards")]
    public SharesiesPortfolioIntroCards? PortfolioIntroCards { get; set; }

    [JsonPropertyName("preferred_name")]
    public string? PreferredName { get; set; }

    [JsonPropertyName("preferred_product")]
    public object? PreferredProduct { get; set; }

    [JsonPropertyName("prescribed_approved")]
    public bool PrescribedApproved { get; set; }

    [JsonPropertyName("prescribed_participant")]
    public string? PrescribedParticipant { get; set; }

    [JsonPropertyName("price_notifications")]
    public Dictionary<string, object>? PriceNotifications { get; set; }

    [JsonPropertyName("recent_searches")]
    public List<string>? RecentSearches { get; set; }

    [JsonPropertyName("registered_preference_at")]
    public string? RegisteredPreferenceAt { get; set; }

    [JsonPropertyName("rwt")]
    public string? Rwt { get; set; }

    [JsonPropertyName("seen_first_time_autoinvest")]
    public bool SeenFirstTimeAutoinvest { get; set; }

    [JsonPropertyName("seen_first_time_investor")]
    public bool SeenFirstTimeInvestor { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("tax_residencies")]
    public List<object>? TaxResidencies { get; set; }

    [JsonPropertyName("tax_year")]
    public int TaxYear { get; set; }

    [JsonPropertyName("tfn_number")]
    public string? TfnNumber { get; set; }

    [JsonPropertyName("transfer_age")]
    public int? TransferAge { get; set; }

    [JsonPropertyName("transfer_age_passed")]
    public bool TransferAgePassed { get; set; }

    [JsonPropertyName("us_equities_enabled")]
    public bool UsEquitiesEnabled { get; set; }

    [JsonPropertyName("us_tax_treaty_status")]
    public string? UsTaxTreatyStatus { get; set; }

    [JsonPropertyName("wallet_balances")]
    public SharesiesWalletBalances? WalletBalances { get; set; }

    [JsonPropertyName("watchlist")]
    public List<string>? Watchlist { get; set; }

    [JsonPropertyName("watchlist_v2")]
    public List<SharesiesWatchlistItem>? WatchlistV2 { get; set; }
}

public class SharesiesHasSeen
{
    [JsonPropertyName("ai_search_beta_terms")]
    public bool AiSearchBetaTerms { get; set; }

    [JsonPropertyName("ai_search_intro")]
    public bool AiSearchIntro { get; set; }

    [JsonPropertyName("airpoints_deposit_in_situ")]
    public bool AirpointsDepositInSitu { get; set; }

    [JsonPropertyName("airpoints_insure_in_situ")]
    public bool AirpointsInsureInSitu { get; set; }

    [JsonPropertyName("airpoints_intro")]
    public bool AirpointsIntro { get; set; }

    [JsonPropertyName("au_idps_at_buy")]
    public bool AuIdpsAtBuy { get; set; }

    [JsonPropertyName("au_idps_at_topup")]
    public bool AuIdpsAtTopup { get; set; }

    [JsonPropertyName("au_kids_accounts_prompt")]
    public bool AuKidsAccountsPrompt { get; set; }

    [JsonPropertyName("au_shares_intro")]
    public bool AuSharesIntro { get; set; }

    [JsonPropertyName("autoinvest")]
    public bool Autoinvest { get; set; }

    [JsonPropertyName("autoinvest_automatic_payment")]
    public bool AutoinvestAutomaticPayment { get; set; }

    [JsonPropertyName("autoinvest_companies_warning")]
    public bool AutoinvestCompaniesWarning { get; set; }

    [JsonPropertyName("autoinvest_promo")]
    public bool AutoinvestPromo { get; set; }

    [JsonPropertyName("autoinvest_splash")]
    public bool AutoinvestSplash { get; set; }

    [JsonPropertyName("blinkpay")]
    public bool Blinkpay { get; set; }

    [JsonPropertyName("companies")]
    public bool Companies { get; set; }

    [JsonPropertyName("completed_topup_goal_tile")]
    public bool CompletedTopupGoalTile { get; set; }

    [JsonPropertyName("create_topup_goal_tile")]
    public bool CreateTopupGoalTile { get; set; }

    [JsonPropertyName("crypto_learn")]
    public bool CryptoLearn { get; set; }

    [JsonPropertyName("dividend_reinvestment_plan")]
    public bool DividendReinvestmentPlan { get; set; }

    [JsonPropertyName("exchange_investor")]
    public bool ExchangeInvestor { get; set; }

    [JsonPropertyName("feedback_activity_feed")]
    public bool FeedbackActivityFeed { get; set; }

    [JsonPropertyName("fonterra_onboarding_screen")]
    public bool FonterraOnboardingScreen { get; set; }

    [JsonPropertyName("fonterra_welcome_screen")]
    public bool FonterraWelcomeScreen { get; set; }

    [JsonPropertyName("funds")]
    public bool Funds { get; set; }

    [JsonPropertyName("grid_vs_list")]
    public bool GridVsList { get; set; }

    [JsonPropertyName("invest_learn")]
    public bool InvestLearn { get; set; }

    [JsonPropertyName("investor")]
    public bool Investor { get; set; }

    [JsonPropertyName("ks_active_welcome_screen")]
    public bool KsActiveWelcomeScreen { get; set; }

    [JsonPropertyName("ks_pending_registration_message_screen")]
    public bool KsPendingRegistrationMessageScreen { get; set; }

    [JsonPropertyName("ks_self_select_education_screens")]
    public bool KsSelfSelectEducationScreens { get; set; }

    [JsonPropertyName("lic_onboarding_screen")]
    public bool LicOnboardingScreen { get; set; }

    [JsonPropertyName("lic_welcome_screen")]
    public bool LicWelcomeScreen { get; set; }

    [JsonPropertyName("limit_orders")]
    public bool LimitOrders { get; set; }

    [JsonPropertyName("managed_funds_investor")]
    public bool ManagedFundsInvestor { get; set; }

    [JsonPropertyName("new_portfolio_diversification_intro")]
    public bool NewPortfolioDiversificationIntro { get; set; }

    [JsonPropertyName("pay_id")]
    public bool PayId { get; set; }

    [JsonPropertyName("plink")]
    public bool Plink { get; set; }

    [JsonPropertyName("portfolio_moved_disclaimer")]
    public bool PortfolioMovedDisclaimer { get; set; }

    [JsonPropertyName("portfolio_view_mode")]
    public bool PortfolioViewMode { get; set; }

    [JsonPropertyName("recurring_topup_autoinvest_tip")]
    public bool RecurringTopupAutoinvestTip { get; set; }

    [JsonPropertyName("rights_intro_modal")]
    public bool RightsIntroModal { get; set; }

    [JsonPropertyName("show_au_currency")]
    public bool ShowAuCurrency { get; set; }

    [JsonPropertyName("sign_up_deposit_prompt")]
    public bool SignUpDepositPrompt { get; set; }

    [JsonPropertyName("spend_credit_alert")]
    public bool SpendCreditAlert { get; set; }

    [JsonPropertyName("spend_google_pay_prompt")]
    public bool SpendGooglePayPrompt { get; set; }

    [JsonPropertyName("us_shares_intro")]
    public bool UsSharesIntro { get; set; }

    [JsonPropertyName("us_shares_resign_intro")]
    public bool UsSharesResignIntro { get; set; }

    [JsonPropertyName("us_shares_resign_welcome_screen")]
    public bool UsSharesResignWelcomeScreen { get; set; }

    [JsonPropertyName("us_shares_welcome_screen")]
    public bool UsSharesWelcomeScreen { get; set; }

    [JsonPropertyName("voting_intro_modal")]
    public bool VotingIntroModal { get; set; }

    [JsonPropertyName("welcome_screens")]
    public bool WelcomeScreens { get; set; }
}

public class SharesiesPortfolioIntroCards
{
    [JsonPropertyName("auto_invest_shown")]
    public bool AutoInvestShown { get; set; }

    [JsonPropertyName("learn_shown")]
    public bool LearnShown { get; set; }
}

public class SharesiesWatchlistItem
{
    [JsonPropertyName("created")]
    public SharesiesQuantumContainer? Created { get; set; }

    [JsonPropertyName("fund_id")]
    public string? FundId { get; set; }
}

public class SharesiesAddress
{
    [JsonPropertyName("formatted")]
    public string? Formatted { get; set; }

    [JsonPropertyName("components")]
    public SharesiesAddressComponents? Components { get; set; }

    [JsonPropertyName("lat")]
    public double? Lat { get; set; }

    [JsonPropertyName("lng")]
    public double? Lng { get; set; }
}

public class SharesiesAddressComponents
{
    [JsonPropertyName("street_number")]
    public string? StreetNumber { get; set; }

    [JsonPropertyName("route")]
    public string? Route { get; set; }

    [JsonPropertyName("sublocality")]
    public string? Sublocality { get; set; }

    [JsonPropertyName("locality")]
    public string? Locality { get; set; }

    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }
}

public class SharesiesUserChecks
{
    [JsonPropertyName("address_entered")]
    public bool AddressEntered { get; set; }

    [JsonPropertyName("address_verified")]
    public bool AddressVerified { get; set; }

    [JsonPropertyName("afsl_migrated")]
    public bool AfslMigrated { get; set; }

    [JsonPropertyName("dependent_declaration")]
    public bool DependentDeclaration { get; set; }

    [JsonPropertyName("fallback_dob_verified")]
    public bool FallbackDobVerified { get; set; }

    [JsonPropertyName("id_verified")]
    public bool IdVerified { get; set; }

    [JsonPropertyName("identity_verification")]
    public SharesiesIdentityVerification? IdentityVerification { get; set; }

    [JsonPropertyName("latest_identity_verification_check")]
    public object? LatestIdentityVerificationCheck { get; set; }

    [JsonPropertyName("made_cumulative_deposits_over_threshold")]
    public bool MadeCumulativeDepositsOverThreshold { get; set; }

    [JsonPropertyName("made_deposit")]
    public bool MadeDeposit { get; set; }

    [JsonPropertyName("nature_and_purpose_answered")]
    public bool NatureAndPurposeAnswered { get; set; }

    [JsonPropertyName("prescribed_answered")]
    public bool PrescribedAnswered { get; set; }

    [JsonPropertyName("tax_questions")]
    public bool TaxQuestions { get; set; }

    [JsonPropertyName("tc_accepted")]
    public bool TcAccepted { get; set; }
}

public class SharesiesIdentityVerification
{
    [JsonPropertyName("additional_verification_required")]
    public bool AdditionalVerificationRequired { get; set; }

    [JsonPropertyName("additional_verification_required_reason")]
    public string? AdditionalVerificationRequiredReason { get; set; }

    [JsonPropertyName("address_entered")]
    public bool AddressEntered { get; set; }

    [JsonPropertyName("address_verified")]
    public bool AddressVerified { get; set; }

    [JsonPropertyName("bank_name_match_verified")]
    public bool BankNameMatchVerified { get; set; }

    [JsonPropertyName("biometric_verified")]
    public bool BiometricVerified { get; set; }

    [JsonPropertyName("id_verified")]
    public bool IdVerified { get; set; }

    [JsonPropertyName("is_identity_linked")]
    public bool IsIdentityLinked { get; set; }

    [JsonPropertyName("latest_biometric_verification_check")]
    public object? LatestBiometricVerificationCheck { get; set; }

    [JsonPropertyName("manual_id_verified")]
    public bool ManualIdVerified { get; set; }

    [JsonPropertyName("name_and_dob_verified")]
    public bool NameAndDobVerified { get; set; }

    [JsonPropertyName("primary_id_type")]
    public string? PrimaryIdType { get; set; }

    [JsonPropertyName("secondary_id_verified")]
    public bool SecondaryIdVerified { get; set; }

    [JsonPropertyName("secondary_identity_document_check")]
    public object? SecondaryIdentityDocumentCheck { get; set; }
}

public class SharesiesWalletBalances
{
    [JsonPropertyName("nzd")]
    public string? Nzd { get; set; }

    [JsonPropertyName("aud")]
    public string? Aud { get; set; }

    [JsonPropertyName("usd")]
    public string? Usd { get; set; }
}