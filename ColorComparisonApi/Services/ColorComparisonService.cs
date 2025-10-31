using ColorComparisonApi.Models;

namespace ColorComparisonApi.Services;

/// <summary>
/// Service for comparing colors against predefined scales (like pool test strips)
/// Uses CIEDE2000 algorithm for perceptually accurate color comparison
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

        scales.Add(new ColorScale
        {
            Name = "Total Hardness",
            ColorPoints = new List<ColorPoint>
            {
                new ColorPoint { Value = 0, HexColor = "#011a94" },
                new ColorPoint { Value = 100, HexColor = "#2539a9" },
                new ColorPoint { Value = 250, HexColor = "#402ba0" },
                new ColorPoint { Value = 500, HexColor = "#873b9c" },
                new ColorPoint { Value = 1000, HexColor = "#8f208b" }
            }
        });

        scales.Add(new ColorScale
        {
            Name = "Total Chlorine",
            ColorPoints = new List<ColorPoint>
            {
                new ColorPoint { Value = 0.0, HexColor = "#fefeaa" },
                new ColorPoint { Value = 0.5, HexColor = "#f3fdaa" },
                new ColorPoint { Value = 1.0, HexColor = "#e7f4a1" },
                new ColorPoint { Value = 3.0, HexColor = "#b8d88c" },
                new ColorPoint { Value = 5.0, HexColor = "#90c676" },
                new ColorPoint { Value = 10.0, HexColor = "#4ba35e" }
            }
        });

        scales.Add(new ColorScale
        {
            Name = "Free Chlorine",
            ColorPoints = new List<ColorPoint>
            {
                new ColorPoint { Value = 0, HexColor = "#fefdcd" },
                new ColorPoint { Value = 0.5, HexColor = "#f8f8df" },
                new ColorPoint { Value = 1.0, HexColor = "#e7dfd7" },
                new ColorPoint { Value = 3.0, HexColor = "#ad8bcf" },
                new ColorPoint { Value = 5.0, HexColor = "#9d69bc" },
                new ColorPoint { Value = 10.0, HexColor = "#801d9a" }
            }
        });

        scales.Add(new ColorScale
        {
            Name = "pH",
            ColorPoints = new List<ColorPoint>
            {
                new ColorPoint { Value = 6.2, HexColor = "#f3af3d" },
                new ColorPoint { Value = 6.8, HexColor = "#e96a2b" },
                new ColorPoint { Value = 7.2, HexColor = "#e03723" },
                new ColorPoint { Value = 7.8, HexColor = "#df3021" },
                new ColorPoint { Value = 8.4, HexColor = "#d62d20" }
            }
        });

        scales.Add(new ColorScale
        {
            Name = "Total Alkalinity",
            ColorPoints = new List<ColorPoint>
            {
                new ColorPoint { Value = 0, HexColor = "#e2c040" },
                new ColorPoint { Value = 40, HexColor = "#a5a934" },
                new ColorPoint { Value = 80, HexColor = "#899f3a" },
                new ColorPoint { Value = 120, HexColor = "#486f36" },
                new ColorPoint { Value = 180, HexColor = "#22522f" },
                new ColorPoint { Value = 240, HexColor = "#245760" }
            }
        });

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

            // Get interpolated value using CIEDE2000 color matching
            var scaleResult = GetInterpolatedValue(inputColor, scale);

            if (scaleResult != null)
            {
                response.ScaleResults.Add(scaleResult);
            }
        }

        return response;
    }

    /// <summary>
    /// Gets the interpolated value for a color on a scale using CIEDE2000 algorithm
    /// and weighted interpolation between the two closest color matches.
    /// </summary>
    private ScaleResult? GetInterpolatedValue(string testColorHex, ColorScale scale)
    {
        if (scale.ColorPoints.Count == 0)
            return null;

        if (scale.ColorPoints.Count == 1)
        {
            var point = scale.ColorPoints[0];
            return new ScaleResult
            {
                ScaleName = scale.Name,
                Value = point.Value,
                MinValue = point.Value,
                MaxValue = point.Value,
                MatchedColor = testColorHex,
                ClosestReferenceColor = point.HexColor,
                ColorDistance = CalculateCIEDE2000(HexToLab(testColorHex), HexToLab(point.HexColor))
            };
        }

        // Convert test color to LAB
        var testLab = HexToLab(testColorHex);

        // Calculate CIEDE2000 distances for all chart colors
        var distances = scale.ColorPoints.Select(point => new
        {
            Point = point,
            Distance = CalculateCIEDE2000(testLab, HexToLab(point.HexColor))
        }).OrderBy(x => x.Distance).ToList();

        // If we have an exact match (or very close), return that value
        if (distances[0].Distance < 1.0) // Threshold for "just noticeable difference"
        {
            return new ScaleResult
            {
                ScaleName = scale.Name,
                Value = distances[0].Point.Value,
                MinValue = scale.ColorPoints.Min(cp => cp.Value),
                MaxValue = scale.ColorPoints.Max(cp => cp.Value),
                MatchedColor = testColorHex,
                ClosestReferenceColor = distances[0].Point.HexColor,
                ColorDistance = distances[0].Distance
            };
        }

        // Use the two closest colors for interpolation
        var closest1 = distances[0];
        var closest2 = distances[1];

        // Weighted interpolation based on inverse distance
        // The closer the color, the more weight it gets
        double weight1 = 1.0 / (closest1.Distance + 0.1); // Add small epsilon to avoid division by zero
        double weight2 = 1.0 / (closest2.Distance + 0.1);
        double totalWeight = weight1 + weight2;

        double interpolatedValue = (closest1.Point.Value * weight1 + closest2.Point.Value * weight2) / totalWeight;

        return new ScaleResult
        {
            ScaleName = scale.Name,
            Value = interpolatedValue,
            MinValue = scale.ColorPoints.Min(cp => cp.Value),
            MaxValue = scale.ColorPoints.Max(cp => cp.Value),
            MatchedColor = testColorHex,
            ClosestReferenceColor = closest1.Point.HexColor,
            ColorDistance = closest1.Distance
        };
    }

    /// <summary>
    /// Calculates the CIEDE2000 color difference between two LAB colors.
    /// Based on the paper "The CIEDE2000 Color-Difference Formula" by Sharma, Wu, and Dalal.
    /// </summary>
    private double CalculateCIEDE2000((double L, double a, double b) lab1,
                                      (double L, double a, double b) lab2)
    {
        // Reference constants
        const double kL = 1.0;
        const double kC = 1.0;
        const double kH = 1.0;
        const double deg360InRad = Math.PI * 2;
        const double deg180InRad = Math.PI;
        const double pow25To7 = 6103515625.0;

        // Calculate C and h
        double C1 = Math.Sqrt(lab1.a * lab1.a + lab1.b * lab1.b);
        double C2 = Math.Sqrt(lab2.a * lab2.a + lab2.b * lab2.b);
        double barC = (C1 + C2) / 2.0;

        double G = 0.5 * (1 - Math.Sqrt(Math.Pow(barC, 7) / (Math.Pow(barC, 7) + pow25To7)));

        double a1Prime = (1.0 + G) * lab1.a;
        double a2Prime = (1.0 + G) * lab2.a;

        double C1Prime = Math.Sqrt(a1Prime * a1Prime + lab1.b * lab1.b);
        double C2Prime = Math.Sqrt(a2Prime * a2Prime + lab2.b * lab2.b);

        double h1Prime = (Math.Atan2(lab1.b, a1Prime) + deg360InRad) % deg360InRad;
        double h2Prime = (Math.Atan2(lab2.b, a2Prime) + deg360InRad) % deg360InRad;

        // Calculate delta values
        double deltaLPrime = lab2.L - lab1.L;
        double deltaCPrime = C2Prime - C1Prime;

        double deltahPrime;
        if (C1Prime * C2Prime == 0)
        {
            deltahPrime = 0;
        }
        else if (Math.Abs(h2Prime - h1Prime) <= deg180InRad)
        {
            deltahPrime = h2Prime - h1Prime;
        }
        else if (h2Prime - h1Prime > deg180InRad)
        {
            deltahPrime = h2Prime - h1Prime - deg360InRad;
        }
        else
        {
            deltahPrime = h2Prime - h1Prime + deg360InRad;
        }

        double deltaHPrime = 2.0 * Math.Sqrt(C1Prime * C2Prime) * Math.Sin(deltahPrime / 2.0);

        // Calculate CIEDE2000
        double barLPrime = (lab1.L + lab2.L) / 2.0;
        double barCPrime = (C1Prime + C2Prime) / 2.0;

        double barhPrime;
        if (C1Prime * C2Prime == 0)
        {
            barhPrime = h1Prime + h2Prime;
        }
        else if (Math.Abs(h1Prime - h2Prime) <= deg180InRad)
        {
            barhPrime = (h1Prime + h2Prime) / 2.0;
        }
        else if (h1Prime + h2Prime < deg360InRad)
        {
            barhPrime = (h1Prime + h2Prime + deg360InRad) / 2.0;
        }
        else
        {
            barhPrime = (h1Prime + h2Prime - deg360InRad) / 2.0;
        }

        double T = 1.0 - 0.17 * Math.Cos(barhPrime - Math.PI / 6.0) +
                   0.24 * Math.Cos(2.0 * barhPrime) +
                   0.32 * Math.Cos(3.0 * barhPrime + Math.PI / 30.0) -
                   0.20 * Math.Cos(4.0 * barhPrime - 7.0 * Math.PI / 20.0);

        double deltaTheta = (Math.PI / 6.0) * Math.Exp(-Math.Pow((barhPrime - 275.0 * Math.PI / 180.0) / (25.0 * Math.PI / 180.0), 2));
        double RC = 2.0 * Math.Sqrt(Math.Pow(barCPrime, 7) / (Math.Pow(barCPrime, 7) + pow25To7));
        double SL = 1.0 + (0.015 * Math.Pow(barLPrime - 50.0, 2)) / Math.Sqrt(20.0 + Math.Pow(barLPrime - 50.0, 2));
        double SC = 1.0 + 0.045 * barCPrime;
        double SH = 1.0 + 0.015 * barCPrime * T;
        double RT = -Math.Sin(2.0 * deltaTheta) * RC;

        double deltaE = Math.Sqrt(
            Math.Pow(deltaLPrime / (kL * SL), 2) +
            Math.Pow(deltaCPrime / (kC * SC), 2) +
            Math.Pow(deltaHPrime / (kH * SH), 2) +
            RT * (deltaCPrime / (kC * SC)) * (deltaHPrime / (kH * SH))
        );

        return deltaE;
    }

    /// <summary>
    /// Converts hex color to LAB color space
    /// </summary>
    private (double L, double a, double b) HexToLab(string hex)
    {
        var rgb = HexToRgb(hex);
        var xyz = RgbToXyz(rgb);
        return XyzToLab(xyz);
    }

    /// <summary>
    /// Converts a hex color code to RGB values (0.0 - 1.0 range)
    /// </summary>
    private (double R, double G, double B) HexToRgb(string hex)
    {
        hex = hex.TrimStart('#');

        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
        int b = Convert.ToInt32(hex.Substring(4, 2), 16);

        return (r / 255.0, g / 255.0, b / 255.0);
    }

    /// <summary>
    /// Converts RGB to XYZ color space using D65 illuminant
    /// </summary>
    private (double X, double Y, double Z) RgbToXyz((double R, double G, double B) rgb)
    {
        // Convert to linear RGB
        double r = rgb.R > 0.04045 ? Math.Pow((rgb.R + 0.055) / 1.055, 2.4) : rgb.R / 12.92;
        double g = rgb.G > 0.04045 ? Math.Pow((rgb.G + 0.055) / 1.055, 2.4) : rgb.G / 12.92;
        double b = rgb.B > 0.04045 ? Math.Pow((rgb.B + 0.055) / 1.055, 2.4) : rgb.B / 12.92;

        // Convert to XYZ using D65 illuminant
        double x = r * 0.4124564 + g * 0.3575761 + b * 0.1804375;
        double y = r * 0.2126729 + g * 0.7151522 + b * 0.0721750;
        double z = r * 0.0193339 + g * 0.1191920 + b * 0.9503041;

        return (x * 100, y * 100, z * 100);
    }

    /// <summary>
    /// Converts XYZ to LAB color space using D65 white point
    /// </summary>
    private (double L, double a, double b) XyzToLab((double X, double Y, double Z) xyz)
    {
        // D65 reference white point
        const double refX = 95.047;
        const double refY = 100.000;
        const double refZ = 108.883;

        double x = xyz.X / refX;
        double y = xyz.Y / refY;
        double z = xyz.Z / refZ;

        x = x > 0.008856 ? Math.Pow(x, 1.0 / 3.0) : (7.787 * x + 16.0 / 116.0);
        y = y > 0.008856 ? Math.Pow(y, 1.0 / 3.0) : (7.787 * y + 16.0 / 116.0);
        z = z > 0.008856 ? Math.Pow(z, 1.0 / 3.0) : (7.787 * z + 16.0 / 116.0);

        double L = 116.0 * y - 16.0;
        double a = 500.0 * (x - y);
        double b = 200.0 * (y - z);

        return (L, a, b);
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
