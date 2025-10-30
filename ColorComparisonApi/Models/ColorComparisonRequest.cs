namespace ColorComparisonApi.Models;

/// <summary>
/// Request model containing 5 hex color codes for comparison
/// </summary>
public class ColorComparisonRequest
{
    /// <summary>
    /// Array of 5 hex color codes (e.g., "#FF5733")
    /// </summary>
    public string[] ColorHexCodes { get; set; } = Array.Empty<string>();
}
