using ColorComparisonApi.Models;

namespace ColorComparisonApi.Services;

/// <summary>
/// Service interface for color comparison operations
/// </summary>
public interface IColorComparisonService
{
    /// <summary>
    /// Compares input colors against predefined scales and returns scale values
    /// </summary>
    /// <param name="colorHexCodes">Array of 5 hex color codes</param>
    /// <returns>Comparison results for all scales</returns>
    ColorComparisonResponse CompareColors(string[] colorHexCodes);
}
