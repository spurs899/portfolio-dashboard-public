using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace PortfolioManager.Web.Services;

public interface ISharesiesService
{
    Task<LoginResult> LoginAsync(string email, string password);
    Task<LoginResult> LoginMfaAsync(string email, string password, string mfaCode);
    Task<ProfileResponse?> GetProfileAsync();
    Task<PortfolioResponse?> GetPortfolioAsync(string userId, string rakaiaToken, string distillToken);
}

public class SharesiesService : ISharesiesService
{
    private readonly HttpClient _httpClient;
    private readonly bool _demoMode;

    public SharesiesService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _demoMode = configuration.GetValue<bool>("DemoMode");
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        if (_demoMode)
        {
            await Task.Delay(800); // Simulate network delay
            
            // Accept any credentials in demo mode
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new LoginResult
                {
                    Success = false,
                    RequiresMfa = false,
                    Message = "Please enter email and password"
                };
            }
            
            return new LoginResult
            {
                Success = true,
                RequiresMfa = false,
                UserId = "demo-user-123",
                RakaiaToken = "demo-rakaia-token",
                DistillToken = "demo-distill-token",
                Message = "Login successful (Demo Mode)"
            };
        }

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
                RakaiaToken = loginResponse?.RakaiaToken,
                DistillToken = loginResponse?.DistillToken,
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
        if (_demoMode)
        {
            await Task.Delay(600);
            
            if (string.IsNullOrWhiteSpace(mfaCode))
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Please enter MFA code"
                };
            }
            
            return new LoginResult
            {
                Success = true,
                UserId = "demo-user-123",
                RakaiaToken = "demo-rakaia-token",
                DistillToken = "demo-distill-token",
                Message = "MFA Login successful (Demo Mode)"
            };
        }

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
                RakaiaToken = loginResponse?.RakaiaToken,
                DistillToken = loginResponse?.DistillToken,
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
        if (_demoMode)
        {
            await Task.Delay(300);
            return new ProfileResponse
            {
                Profiles = new List<Profile>
                {
                    new Profile
                    {
                        Id = "demo-profile-1",
                        Name = "Demo Investment Account",
                        Portfolios = new List<Portfolio>
                        {
                            new Portfolio
                            {
                                Id = "demo-portfolio-1",
                                Balance = "5000.00",
                                HoldingBalance = "45000.00"
                            }
                        }
                    }
                }
            };
        }

        return await _httpClient.GetFromJsonAsync<ProfileResponse>("api/Sharesies/profile");
    }

    public async Task<PortfolioResponse?> GetPortfolioAsync(string userId, string rakaiaToken, string distillToken)
    {
        if (_demoMode)
        {
            await Task.Delay(500);
            return new PortfolioResponse
            {
                UserProfile = new UserProfileDto
                {
                    Id = "demo-user-123",
                    Name = "Demo Investor",
                    Image = "https://via.placeholder.com/150",
                    BrokerageType = 1
                },
                Instruments = new List<InstrumentDto>
                {
                    new InstrumentDto
                    {
                        Id = "1",
                        Symbol = "AAPL",
                        Name = "Apple Inc.",
                        Currency = "USD",
                        BrokerageType = 1,
                        SharesOwned = 50,
                        SharePrice = 175.50m,
                        InvestmentValue = 8775.00m,
                        CostBasis = 7500.00m,
                        TotalReturn = 1275.00m,
                        SimpleReturn = 17.00m,
                        DividendsReceived = 125.00m
                    },
                    new InstrumentDto
                    {
                        Id = "2",
                        Symbol = "MSFT",
                        Name = "Microsoft Corporation",
                        Currency = "USD",
                        BrokerageType = 0,
                        SharesOwned = 75,
                        SharePrice = 380.00m,
                        InvestmentValue = 28500.00m,
                        CostBasis = 25000.00m,
                        TotalReturn = 3500.00m,
                        SimpleReturn = 14.00m,
                        DividendsReceived = 450.00m
                    },
                    new InstrumentDto
                    {
                        Id = "3",
                        Symbol = "GOOGL",
                        Name = "Alphabet Inc.",
                        Currency = "USD",
                        BrokerageType = 1,
                        SharesOwned = 30,
                        SharePrice = 140.25m,
                        InvestmentValue = 4207.50m,
                        CostBasis = 4500.00m,
                        TotalReturn = -292.50m,
                        SimpleReturn = -6.50m,
                        DividendsReceived = 0.00m
                    },
                    new InstrumentDto
                    {
                        Id = "4",
                        Symbol = "TSLA",
                        Name = "Tesla Inc.",
                        Currency = "USD",
                        BrokerageType = 1,
                        SharesOwned = 20,
                        SharePrice = 245.00m,
                        InvestmentValue = 4900.00m,
                        CostBasis = 3800.00m,
                        TotalReturn = 1100.00m,
                        SimpleReturn = 28.95m,
                        DividendsReceived = 0.00m
                    },
                    new InstrumentDto
                    {
                        Id = "5",
                        Symbol = "NVDA",
                        Name = "NVIDIA Corporation",
                        Currency = "USD",
                        BrokerageType = 0,
                        SharesOwned = 15,
                        SharePrice = 495.00m,
                        InvestmentValue = 7425.00m,
                        CostBasis = 4200.00m,
                        TotalReturn = 3225.00m,
                        SimpleReturn = 76.79m,
                        DividendsReceived = 25.00m
                    }
                }
            };
        }

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/Sharesies/portfolio?userId={userId}");
            request.Headers.Add("X-Rakaia-Token", rakaiaToken);
            request.Headers.Add("X-Distill-Token", distillToken);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PortfolioResponse>();
            }
            
            return null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Return null to indicate authentication failure
            return null;
        }
    }
}

public class LoginResult
{
    public bool Success { get; set; }
    public bool RequiresMfa { get; set; }
    public string? UserId { get; set; }
    public string? Message { get; set; }
    public string? RakaiaToken { get; set; }
    public string? DistillToken { get; set; }
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
    
    [JsonPropertyName("rakaia_token")]
    public string? RakaiaToken { get; set; }
    
    [JsonPropertyName("distill_token")]
    public string? DistillToken { get; set; }
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
