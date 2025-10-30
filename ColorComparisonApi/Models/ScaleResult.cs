namespace ColorComparisonApi.Models;

/// <summary>
/// Result for a single scale comparison
/// </summary>
public class ScaleResult
{
    /// <summary>
    /// Name of the scale (e.g., "pH", "Chlorine", "Alkalinity")
    /// </summary>
    public string ScaleName { get; set; } = string.Empty;

    /// <summary>
    /// The computed value on this scale based on color comparison
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// The minimum value for this scale
    /// </summary>
    public double MinValue { get; set; }

    /// <summary>
    /// The maximum value for this scale
    /// </summary>
    public double MaxValue { get; set; }

    /// <summary>
    /// The hex color that was matched from the input
    /// </summary>
    public string MatchedColor { get; set; } = string.Empty;

    /// <summary>
    /// The reference hex color from the scale definition that was closest
    /// </summary>
    public string ClosestReferenceColor { get; set; } = string.Empty;

    /// <summary>
    /// Distance/difference metric between the matched color and reference color
    /// </summary>
    public double ColorDistance { get; set; }
}
