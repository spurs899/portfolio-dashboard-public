using System.Net.Http.Json;
using PortfolioManager.Contracts;
using PortfolioManager.Contracts.Models.Brokerage;
using PortfolioManager.Contracts.Web;

namespace PortfolioManager.Web.Services;

public interface IBrokerageService
{
    Task<BrokerageAuthResult> AuthenticateAsync(string brokerageType, string username, string password);
    Task<BrokerageAuthResult> ContinueAuthenticationAsync(string brokerageType, AuthenticationContinuation continuation);
    Task<PortfolioResponse?> GetPortfolioAsync(AuthenticationResult authResult);
    Task<List<SupportedBrokerage>> GetSupportedBrokeragesAsync();
}

public class BrokerageAuthResult
{
    public bool Success { get; set; }
    public bool Authenticated { get; set; }
    public AuthenticationStep Step { get; set; }
    public string? SessionId { get; set; }
    public string? UserId { get; set; }
    public Dictionary<string, string>? Tokens { get; set; }
    public string? Message { get; set; }
    
    // MFA
    public bool RequiresMfa { get; set; }
    public string? MfaType { get; set; }
    
    // QR Code
    public bool RequiresQrScan { get; set; }
    public string? QrCodeBase64 { get; set; }
}

public class AuthenticationContinuation
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string? MfaCode { get; set; }
}

public class SupportedBrokerage
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class BrokerageService : IBrokerageService
{
    private readonly HttpClient _httpClient;
    private readonly bool _demoMode;

    public BrokerageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _demoMode = configuration.GetValue<bool>("DemoMode");
    }

    public async Task<BrokerageAuthResult> AuthenticateAsync(string brokerageType, string username, string password)
    {
        if (_demoMode)
        {
            return await HandleDemoAuth(username, password);
        }

        try
        {
            var credentials = new
            {
                username,
                password
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"api/brokerage/{brokerageType}/authenticate",
                credentials);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BrokerageAuthResponse>();
                
                return new BrokerageAuthResult
                {
                    Success = result?.Success ?? false,
                    Authenticated = result?.Authenticated ?? false,
                    Step = ParseStep(result?.Step),
                    SessionId = result?.SessionId,
                    UserId = result?.UserId,
                    Tokens = result?.Tokens,
                    Message = result?.Message,
                    RequiresMfa = result?.RequiresMfa ?? false,
                    MfaType = result?.MfaType,
                    RequiresQrScan = result?.RequiresQrScan ?? false,
                    QrCodeBase64 = result?.QrCodeBase64
                };
            }

            var errorResult = await response.Content.ReadFromJsonAsync<BrokerageAuthResponse>();
            return new BrokerageAuthResult
            {
                Success = false,
                Message = errorResult?.Message ?? "Authentication failed"
            };
        }
        catch (Exception ex)
        {
            return new BrokerageAuthResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<BrokerageAuthResult> ContinueAuthenticationAsync(
        string brokerageType,
        AuthenticationContinuation continuation)
    {
        if (_demoMode)
        {
            return await HandleDemoMfaAuth(continuation.MfaCode);
        }

        try
        {
            var credentials = new
            {
                continuation.Username,
                continuation.Password,
                continuation.SessionId,
                continuation.MfaCode
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"api/brokerage/{brokerageType}/authenticate/continue",
                credentials);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BrokerageAuthResponse>();
                
                return new BrokerageAuthResult
                {
                    Success = result?.Success ?? false,
                    Authenticated = result?.Authenticated ?? false,
                    Step = ParseStep(result?.Step),
                    SessionId = result?.SessionId,
                    UserId = result?.UserId,
                    Tokens = result?.Tokens,
                    Message = result?.Message,
                    RequiresMfa = result?.RequiresMfa ?? false,
                    RequiresQrScan = result?.RequiresQrScan ?? false
                };
            }

            var errorResult = await response.Content.ReadFromJsonAsync<BrokerageAuthResponse>();
            return new BrokerageAuthResult
            {
                Success = false,
                Message = errorResult?.Message ?? "Authentication continuation failed"
            };
        }
        catch (Exception ex)
        {
            return new BrokerageAuthResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<PortfolioResponse?> GetPortfolioAsync(AuthenticationResult authResult)
    {
        if (_demoMode)
        {
            return GetDemoPortfolio();
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/brokerage/sharesies/portfolio", // For now, hardcoded to sharesies
                authResult);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PortfolioResponse>();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<SupportedBrokerage>> GetSupportedBrokeragesAsync()
    {
        if (_demoMode)
        {
            return new List<SupportedBrokerage>
            {
                new() { Type = Constants.BrokerageSharesies, Name = Constants.BrokerageSharesies },
                new() { Type = Constants.BrokerageIbkr, Name = Constants.BrokerageIbkrName }
            };
        }

        try
        {
            var response = await _httpClient.GetAsync("api/brokerage/supported");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<SupportedBrokerage>>() 
                       ?? new List<SupportedBrokerage>();
            }

            return new List<SupportedBrokerage>();
        }
        catch
        {
            return new List<SupportedBrokerage>();
        }
    }

    private static AuthenticationStep ParseStep(string? step)
    {
        if (string.IsNullOrEmpty(step))
            return AuthenticationStep.Failed;

        return Enum.TryParse<AuthenticationStep>(step, out var result) 
            ? result 
            : AuthenticationStep.Failed;
    }

    private async Task<BrokerageAuthResult> HandleDemoAuth(string username, string password)
    {
        await Task.Delay(800);

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return new BrokerageAuthResult
            {
                Success = false,
                Message = "Please enter username and password"
            };
        }

        return new BrokerageAuthResult
        {
            Success = true,
            Authenticated = true,
            Step = AuthenticationStep.Completed,
            UserId = "demo-user-123",
            Tokens = new Dictionary<string, string>
            {
                ["RakaiaToken"] = "demo-rakaia-token",
                ["DistillToken"] = "demo-distill-token"
            },
            Message = "Login successful (Demo Mode)"
        };
    }

    private async Task<BrokerageAuthResult> HandleDemoMfaAuth(string? mfaCode)
    {
        await Task.Delay(600);

        if (string.IsNullOrWhiteSpace(mfaCode))
        {
            return new BrokerageAuthResult
            {
                Success = false,
                Message = "Please enter MFA code"
            };
        }

        return new BrokerageAuthResult
        {
            Success = true,
            Authenticated = true,
            Step = AuthenticationStep.Completed,
            UserId = "demo-user-123",
            Tokens = new Dictionary<string, string>
            {
                ["RakaiaToken"] = "demo-rakaia-token",
                ["DistillToken"] = "demo-distill-token"
            },
            Message = "MFA authentication successful (Demo Mode)"
        };
    }

    private PortfolioResponse GetDemoPortfolio()
    {
        return new PortfolioResponse
        {
            UserProfile = new UserProfileDto
            {
                Id = "demo-user-123",
                Name = "Demo Investor",
                Image = "https://via.placeholder.com/150",
                BrokerageType = 0
            },
            Instruments = GenerateDemoInstruments()
        };
    }

    private List<InstrumentDto> GenerateDemoInstruments()
    {
        return new List<InstrumentDto>
        {
            new()
            {
                Id = "1", Symbol = "AAPL", Name = "Apple Inc.", Currency = "USD",
                BrokerageType = 0, SharesOwned = 50, SharePrice = 175.50m,
                InvestmentValue = 8775.00m, CostBasis = 7500.00m, TotalReturn = 1275.00m,
                SimpleReturn = 87.75m, DividendsReceived = 125.00m
            },
            new()
            {
                Id = "2", Symbol = "AAPL", Name = "Apple Inc.", Currency = "USD",
                BrokerageType = 1, SharesOwned = 100, SharePrice = 175.50m,
                InvestmentValue = 17550.00m, CostBasis = 16000.00m, TotalReturn = 1550.00m,
                SimpleReturn = 175.50m, DividendsReceived = 250.00m
            },
            new()
            {
                Id = "3", Symbol = "MSFT", Name = "Microsoft Corporation", Currency = "USD",
                BrokerageType = 0, SharesOwned = 75, SharePrice = 380.00m,
                InvestmentValue = 28500.00m, CostBasis = 25500.00m, TotalReturn = 3000.00m,
                SimpleReturn = 285.00m, DividendsReceived = 450.00m
            }
        };
    }

    // Response models for API deserialization
    private class BrokerageAuthResponse
    {
        public bool Success { get; set; }
        public bool Authenticated { get; set; }
        public string? Step { get; set; }
        public string? SessionId { get; set; }
        public string? UserId { get; set; }
        public Dictionary<string, string>? Tokens { get; set; }
        public string? Message { get; set; }
        public bool RequiresMfa { get; set; }
        public string? MfaType { get; set; }
        public bool RequiresQrScan { get; set; }
        public string? QrCodeBase64 { get; set; }
    }
}
