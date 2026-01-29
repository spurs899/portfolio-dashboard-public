using Microsoft.AspNetCore.Components;

namespace PortfolioManager.Web.Components;

public partial class CurrencyFlag
{
    [Parameter]
    public string Currency { get; set; } = "USD";
    
    [Parameter]
    public string Size { get; set; } = "medium"; // small, medium, large
    
    [Parameter]
    public bool IsSelected { get; set; }
    
    [Parameter]
    public EventCallback OnClick { get; set; }

    private string GetContainerStyle()
    {
        var dimension = Size switch
        {
            "small" => "24px",
            "large" => "48px",
            _ => "32px"
        };
        
        var cursor = OnClick.HasDelegate ? "cursor: pointer;" : "";
        var opacity = IsSelected ? "opacity: 1;" : "opacity: 0.6;";
        var transition = "transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);";
        var scale = IsSelected ? "transform: scale(1.05);" : "";
        
        return $"width: {dimension}; height: auto; position: relative; display: inline-block; {cursor} {opacity} {transition} {scale}";
    }
    
    private string GetImageStyle()
    {
        return "width: 100%; height: auto; border-radius: 4px; box-shadow: 0 2px 4px rgba(0,0,0,0.2); display: block; transition: all 0.3s ease;";
    }
    
    private string GetFlagImagePath()
    {
        return Currency switch
        {
            "USD" => "images/us-flag.svg",
            "NZD" => "images/nz-flag.svg",
            _ => "images/us-flag.svg"
        };
    }
}
