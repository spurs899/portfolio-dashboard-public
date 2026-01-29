using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Web;

public class ProfileResponse
{
    [JsonPropertyName("profiles")]
    public List<Profile>? Profiles { get; set; }
}