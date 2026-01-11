using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Web;

public class PortfolioResponse
{
    [JsonPropertyName("userProfile")]
    public UserProfileDto? UserProfile { get; set; }

    [JsonPropertyName("instruments")]
    public List<InstrumentDto>? Instruments { get; set; }
}