using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PortfolioManager.Web.Services;

public interface ISharesiesService
{
    Task<LoginResult> LoginAsync(string email, string password);
    Task<LoginResult> LoginMfaAsync(string email, string password, string mfaCode);
    Task<ProfileResponse?> GetProfileAsync();
    Task<PortfolioResponse?> GetPortfolioAsync(string userId);
}

public class SharesiesService : ISharesiesService
{
    private readonly HttpClient _httpClient;

    public SharesiesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var formData = new Dictionary<string, string>
        {
            { "email", email },
            { "password", password }
        };

        var response = await _httpClient.PostAsync("api/Sharesies/login", new FormUrlEncodedContent(formData));
        
        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return new LoginResult
            {
                Success = loginResponse?.Authenticated ?? false,
                RequiresMfa = false,
                UserId = loginResponse?.User?.Id,
                Message = "Login successful"
            };
        }
        
        // Handle 401 responses which could be MFA required or invalid credentials
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return new LoginResult
            {
                Success = false,
                RequiresMfa = errorResponse?.RequiresMfa ?? false,
                Message = errorResponse?.Message ?? "Login failed"
            };
        }

        return new LoginResult { Success = false, Message = "Login failed" };
    }

    public async Task<LoginResult> LoginMfaAsync(string email, string password, string mfaCode)
    {
        var formData = new Dictionary<string, string>
        {
            { "email", email },
            { "password", password },
            { "mfaCode", mfaCode }
        };

        var response = await _httpClient.PostAsync("api/Sharesies/login/mfa", new FormUrlEncodedContent(formData));
        
        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return new LoginResult
            {
                Success = loginResponse?.Authenticated ?? false,
                UserId = loginResponse?.User?.Id,
                Message = "Login successful"
            };
        }
        
        // Handle 401 responses
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return new LoginResult
            {
                Success = false,
                Message = errorResponse?.Message ?? "MFA verification failed"
            };
        }

        return new LoginResult { Success = false, Message = "MFA login failed" };
    }

    public async Task<ProfileResponse?> GetProfileAsync()
    {
        return await _httpClient.GetFromJsonAsync<ProfileResponse>("api/Sharesies/profile");
    }

    public async Task<PortfolioResponse?> GetPortfolioAsync(string userId)
    {
        return await _httpClient.GetFromJsonAsync<PortfolioResponse>($"api/Sharesies/portfolio?userId={userId}");
    }
}

public class LoginResult
{
    public bool Success { get; set; }
    public bool RequiresMfa { get; set; }
    public string? UserId { get; set; }
    public string? Message { get; set; }
}

public class ApiErrorResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("requiresMfa")]
    public bool RequiresMfa { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class LoginResponse
{
    [JsonPropertyName("authenticated")]
    public bool Authenticated { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("user")]
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }
}

public class ProfileResponse
{
    [JsonPropertyName("profiles")]
    public List<Profile>? Profiles { get; set; }
}

public class Profile
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("portfolios")]
    public List<Portfolio>? Portfolios { get; set; }
}

public class Portfolio
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("balance")]
    public string? Balance { get; set; }

    [JsonPropertyName("holding_balance")]
    public string? HoldingBalance { get; set; }
}

public class PortfolioResponse
{
    [JsonPropertyName("userProfile")]
    public UserProfileDto? UserProfile { get; set; }

    [JsonPropertyName("instruments")]
    public List<InstrumentDto>? Instruments { get; set; }
}

public class UserProfileDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("brokerageType")]
    public int BrokerageType { get; set; }
}

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
