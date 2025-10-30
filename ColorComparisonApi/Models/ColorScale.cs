namespace ColorComparisonApi.Models;

/// <summary>
/// Represents a single color scale with reference points (like a pool test strip scale)
/// </summary>
public class ColorScale
{
    /// <summary>
    /// Name of the scale (e.g., "pH", "Chlorine", "Alkalinity", "Hardness", "Stabilizer")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Reference color points on this scale
    /// </summary>
    public List<ColorPoint> ColorPoints { get; set; } = new();
}

/// <summary>
/// A reference point on a color scale, mapping a value to a color
/// </summary>
public class ColorPoint
{
    /// <summary>
    /// The value this color represents on the scale
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// The hex color code for this reference point (e.g., "#FF5733")
    /// </summary>
    public string HexColor { get; set; } = string.Empty;
}
