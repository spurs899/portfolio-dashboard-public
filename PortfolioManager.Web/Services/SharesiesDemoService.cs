using PortfolioManager.Contracts;
using PortfolioManager.Contracts.Models.Brokerage;
using PortfolioManager.Contracts.Web;
using PortfolioManager.Web.Models;

namespace PortfolioManager.Web.Services;

public class SharesiesDemoService : ISharesiesService
{
    public async Task<BrokerageAuthResult> AuthenticateAsync(string brokerageType, string username, string password)
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

        // In demo mode, always require MFA to demonstrate the full flow
        return new BrokerageAuthResult
        {
            Success = true,
            Authenticated = false,
            RequiresMfa = true,
            MfaType = "Email",
            Step = AuthenticationStep.MfaRequired,
            Message = "MFA code sent (Demo Mode - enter any code)"
        };
    }

    public async Task<BrokerageAuthResult> ContinueAuthenticationAsync(string brokerageType, AuthenticationContinuation continuation)
    {
        await Task.Delay(600);

        if (string.IsNullOrWhiteSpace(continuation.MfaCode))
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

    public async Task<PortfolioResponse?> GetPortfolioAsync(AuthenticationResult authResult)
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

    public async Task<List<SupportedBrokerage>> GetSupportedBrokeragesAsync()
    {
        return new List<SupportedBrokerage>
        {
            new() { Type = Constants.BrokerageSharesies, Name = Constants.BrokerageSharesies },
            new() { Type = Constants.BrokerageIbkr, Name = Constants.BrokerageIbkrName }
        };
    }
}