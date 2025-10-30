namespace ColorComparisonApi.Models;

/// <summary>
/// Response model containing results for all 5 scale comparisons
/// </summary>
public class ColorComparisonResponse
{
    /// <summary>
    /// Results for each of the 5 scales
    /// </summary>
    public List<ScaleResult> ScaleResults { get; set; } = new();

    /// <summary>
    /// Overall success status
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Any error or warning messages
    /// </summary>
    public string? Message { get; set; }
}
