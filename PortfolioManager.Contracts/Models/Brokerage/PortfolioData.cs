using PortfolioManager.Contracts.Models.Shared;

namespace PortfolioManager.Contracts.Models.Brokerage;

public class PortfolioData
{
    public required UserProfile UserProfile { get; set; }
    public required List<PortfolioInstrument> Instruments { get; set; }
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
}
