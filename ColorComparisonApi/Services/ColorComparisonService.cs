using ColorComparisonApi.Models;

namespace ColorComparisonApi.Services;

/// <summary>
/// Service for comparing colors against predefined scales (like pool test strips)
/// </summary>
public class ColorComparisonService : IColorComparisonService
{
    private readonly List<ColorScale> _scales;

    public ColorComparisonService()
    {
        _scales = InitializeScales();
    }

    /// <summary>
    /// Initialize the 5 color scales with their reference colors and values
    /// </summary>
    private List<ColorScale> InitializeScales()
    {
        var scales = new List<ColorScale>();

        // TODO: Define Scale 1 - Example: pH Scale
        // Example structure:
        // scales.Add(new ColorScale
        // {
        //     Name = "pH",
        //     ColorPoints = new List<ColorPoint>
        //     {
        //         new ColorPoint { Value = 6.0, HexColor = "#FFFF00" },  // Yellow
        //         new ColorPoint { Value = 6.5, HexColor = "#FFE600" },
        //         new ColorPoint { Value = 7.0, HexColor = "#FFC800" },
        //         new ColorPoint { Value = 7.5, HexColor = "#FFA500" },
        //         new ColorPoint { Value = 8.0, HexColor = "#FF8C00" },
        //         new ColorPoint { Value = 8.5, HexColor = "#FF6600" }   // Orange
        //     }
        // });

        // TODO: Define Scale 2 - Example: Chlorine
        // scales.Add(new ColorScale
        // {
        //     Name = "Chlorine",
        //     ColorPoints = new List<ColorPoint>
        //     {
        //         new ColorPoint { Value = 0.0, HexColor = "#FFFFFF" },   // White
        //         new ColorPoint { Value = 0.5, HexColor = "#FFFFCC" },
        //         new ColorPoint { Value = 1.0, HexColor = "#FFFF99" },   // Light yellow
        //         new ColorPoint { Value = 3.0, HexColor = "#FFFF00" },   // Yellow
        //         new ColorPoint { Value = 5.0, HexColor = "#FFD700" },
        //         new ColorPoint { Value = 10.0, HexColor = "#FFA500" }   // Orange
        //     }
        // });

        // TODO: Define Scale 3 - Example: Alkalinity
        // scales.Add(new ColorScale
        // {
        //     Name = "Alkalinity",
        //     ColorPoints = new List<ColorPoint>
        //     {
        //         new ColorPoint { Value = 0, HexColor = "#E0F7FA" },     // Very light cyan
        //         new ColorPoint { Value = 40, HexColor = "#B2EBF2" },
        //         new ColorPoint { Value = 80, HexColor = "#80DEEA" },
        //         new ColorPoint { Value = 120, HexColor = "#4DD0E1" },
        //         new ColorPoint { Value = 180, HexColor = "#26C6DA" },
        //         new ColorPoint { Value = 240, HexColor = "#00BCD4" }    // Cyan
        //     }
        // });

        // TODO: Define Scale 4 - Example: Hardness
        // scales.Add(new ColorScale
        // {
        //     Name = "Hardness",
        //     ColorPoints = new List<ColorPoint>
        //     {
        //         new ColorPoint { Value = 0, HexColor = "#F3E5F5" },     // Very light purple
        //         new ColorPoint { Value = 50, HexColor = "#E1BEE7" },
        //         new ColorPoint { Value = 100, HexColor = "#CE93D8" },
        //         new ColorPoint { Value = 250, HexColor = "#BA68C8" },
        //         new ColorPoint { Value = 500, HexColor = "#AB47BC" },
        //         new ColorPoint { Value = 1000, HexColor = "#9C27B0" }   // Purple
        //     }
        // });

        // TODO: Define Scale 5 - Example: Stabilizer/Cyanuric Acid
        // scales.Add(new ColorScale
        // {
        //     Name = "Stabilizer",
        //     ColorPoints = new List<ColorPoint>
        //     {
        //         new ColorPoint { Value = 0, HexColor = "#E8F5E9" },     // Very light green
        //         new ColorPoint { Value = 10, HexColor = "#C8E6C9" },
        //         new ColorPoint { Value = 30, HexColor = "#A5D6A7" },
        //         new ColorPoint { Value = 50, HexColor = "#81C784" },
        //         new ColorPoint { Value = 100, HexColor = "#66BB6A" },
        //         new ColorPoint { Value = 150, HexColor = "#4CAF50" }    // Green
        //     }
        // });

        return scales;
    }

    /// <summary>
    /// Compares input colors against predefined scales
    /// </summary>
    public ColorComparisonResponse CompareColors(string[] colorHexCodes)
    {
        var response = new ColorComparisonResponse
        {
            Success = true,
            ScaleResults = new List<ScaleResult>()
        };

        // Validate input
        if (colorHexCodes == null || colorHexCodes.Length != 5)
        {
            response.Success = false;
            response.Message = "Exactly 5 hex color codes are required.";
            return response;
        }

        // Validate hex codes
        for (int i = 0; i < colorHexCodes.Length; i++)
        {
            if (!IsValidHexColor(colorHexCodes[i]))
            {
                response.Success = false;
                response.Message = $"Invalid hex color code at position {i + 1}: {colorHexCodes[i]}";
                return response;
            }
        }

        // Check if scales are defined
        if (_scales.Count == 0)
        {
            response.Success = false;
            response.Message = "No color scales have been defined. Please add scale definitions in the InitializeScales method.";
            return response;
        }

        if (_scales.Count != 5)
        {
            response.Success = false;
            response.Message = $"Expected 5 scales, but {_scales.Count} are defined. Please define exactly 5 scales.";
            return response;
        }

        // Compare each color against its corresponding scale
        for (int i = 0; i < 5; i++)
        {
            var scale = _scales[i];
            var inputColor = colorHexCodes[i];

            // Find the closest matching color point on this scale
            var closestPoint = FindClosestColorPoint(inputColor, scale);

            if (closestPoint != null)
            {
                var scaleResult = new ScaleResult
                {
                    ScaleName = scale.Name,
                    Value = closestPoint.Value,
                    MinValue = scale.ColorPoints.Min(cp => cp.Value),
                    MaxValue = scale.ColorPoints.Max(cp => cp.Value),
                    MatchedColor = inputColor,
                    ClosestReferenceColor = closestPoint.HexColor,
                    ColorDistance = CalculateColorDistance(inputColor, closestPoint.HexColor)
                };

                response.ScaleResults.Add(scaleResult);
            }
        }

        return response;
    }

    /// <summary>
    /// Finds the closest color point on a scale to the input color
    /// </summary>
    private ColorPoint? FindClosestColorPoint(string inputHex, ColorScale scale)
    {
        if (scale.ColorPoints.Count == 0)
            return null;

        ColorPoint? closestPoint = null;
        double minDistance = double.MaxValue;

        foreach (var point in scale.ColorPoints)
        {
            double distance = CalculateColorDistance(inputHex, point.HexColor);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = point;
            }
        }

        return closestPoint;
    }

    /// <summary>
    /// Calculates the Euclidean distance between two colors in RGB space
    /// </summary>
    private double CalculateColorDistance(string hex1, string hex2)
    {
        var rgb1 = HexToRgb(hex1);
        var rgb2 = HexToRgb(hex2);

        // Euclidean distance in RGB color space
        double rDiff = rgb1.R - rgb2.R;
        double gDiff = rgb1.G - rgb2.G;
        double bDiff = rgb1.B - rgb2.B;

        return Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
    }

    /// <summary>
    /// Converts a hex color code to RGB values
    /// </summary>
    private (int R, int G, int B) HexToRgb(string hex)
    {
        // Remove # if present
        hex = hex.TrimStart('#');

        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
        int b = Convert.ToInt32(hex.Substring(4, 2), 16);

        return (r, g, b);
    }

    /// <summary>
    /// Validates if a string is a valid hex color code
    /// </summary>
    private bool IsValidHexColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return false;

        // Remove # if present
        hex = hex.TrimStart('#');

        // Must be exactly 6 characters
        if (hex.Length != 6)
            return false;

        // Must be valid hexadecimal
        return System.Text.RegularExpressions.Regex.IsMatch(hex, "^[0-9A-Fa-f]{6}$");
    }
}
