# Color Comparison API

A .NET 8.0 Web API for comparing colors against predefined scales, similar to pool test strip analysis.

## Overview

This API accepts 5 hex color codes and compares each one against a corresponding scale (like pH, Chlorine, Alkalinity, etc. in pool testing). It returns the computed value on each scale based on color matching.

## Endpoint

### POST `/api/ColorComparison/compare`

Compares 5 hex color codes against 5 predefined scales.

**Request Body:**
```json
{
  "colorHexCodes": [
    "#FFFF00",
    "#FFFF99",
    "#80DEEA",
    "#CE93D8",
    "#A5D6A7"
  ]
}
```

**Response:**
```json
{
  "scaleResults": [
    {
      "scaleName": "pH",
      "value": 6.0,
      "minValue": 6.0,
      "maxValue": 8.5,
      "matchedColor": "#FFFF00",
      "closestReferenceColor": "#FFFF00",
      "colorDistance": 0.0
    },
    // ... 4 more scale results
  ],
  "success": true,
  "message": null
}
```

## Configuration

### Defining Color Scales

The scales are defined in `Services/ColorComparisonService.cs` in the `InitializeScales()` method. You need to:

1. **Define 5 scales** (one for each input color)
2. **Each scale needs:**
   - A name (e.g., "pH", "Chlorine", "Alkalinity", "Hardness", "Stabilizer")
   - Multiple color points (reference colors with their corresponding values)

### Example Scale Definition

Look for the TODO comments in `ColorComparisonService.cs` and add your scale definitions:

```csharp
scales.Add(new ColorScale
{
    Name = "pH",
    ColorPoints = new List<ColorPoint>
    {
        new ColorPoint { Value = 6.0, HexColor = "#FFFF00" },  // Yellow
        new ColorPoint { Value = 6.5, HexColor = "#FFE600" },
        new ColorPoint { Value = 7.0, HexColor = "#FFC800" },
        new ColorPoint { Value = 7.5, HexColor = "#FFA500" },
        new ColorPoint { Value = 8.0, HexColor = "#FF8C00" },
        new ColorPoint { Value = 8.5, HexColor = "#FF6600" }   // Orange
    }
});
```

## Running the Application

### Prerequisites
- .NET 8.0 SDK

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

### Test with curl
```bash
curl -X POST "https://localhost:5001/api/ColorComparison/compare" \
  -H "Content-Type: application/json" \
  -d '{
    "colorHexCodes": [
      "#FFFF00",
      "#FFFF99",
      "#80DEEA",
      "#CE93D8",
      "#A5D6A7"
    ]
  }'
```

## How It Works

1. **Input Validation**: Validates that exactly 5 valid hex color codes are provided
2. **Color Matching**: For each input color, uses the CIEDE2000 algorithm to calculate perceptually accurate color differences against all reference colors in the corresponding scale
3. **Value Calculation**: 
   - If the color is very close to a reference color (deltaE < 1.0), returns that exact value
   - Otherwise, performs weighted interpolation between the two closest reference colors based on their CIEDE2000 distances
4. **Response**: Returns results for all 5 scales with the interpolated values and color information

### CIEDE2000 Algorithm

This API uses the CIEDE2000 color difference formula, which provides perceptually uniform color comparisons. Unlike simple RGB distance, CIEDE2000:

- **Converts colors to LAB color space** (via RGB → XYZ → LAB transformation)
- **Accounts for human perception** of color differences
- **Provides accurate matching** across different hues, saturations, and lightnesses
- **Industry standard** for color quality control and matching

### Interpolation

Instead of simply returning the value of the closest color match, the API interpolates between the two closest matches using weighted averaging:

- **Weight calculation**: Uses inverse distance weighting, so closer colors contribute more to the final value
- **Smooth gradients**: Provides values between reference points for colors that fall "in between"
- **More accurate**: Better represents the actual measurement value for intermediate colors

**Example**: If a test color falls between yellow (0 ppm) and green (1.0 ppm), and is closer to yellow, the result might be 0.35 ppm rather than snapping to either 0 or 1.0.

## Project Structure

```
ColorComparisonApi/
├── Controllers/
│   └── ColorComparisonController.cs    # API endpoint
├── Models/
│   ├── ColorComparisonRequest.cs       # Request DTO
│   ├── ColorComparisonResponse.cs      # Response DTO
│   ├── ScaleResult.cs                  # Individual scale result
│   └── ColorScale.cs                   # Scale and color point definitions
├── Services/
│   ├── IColorComparisonService.cs      # Service interface
│   └── ColorComparisonService.cs       # Color comparison logic (TODO: Define scales here)
└── Program.cs                           # Application entry point
```

## TODO

Before using the API, you must define the 5 color scales in `Services/ColorComparisonService.cs`:
- [ ] Define Scale 1 (e.g., pH)
- [ ] Define Scale 2 (e.g., Chlorine)
- [ ] Define Scale 3 (e.g., Alkalinity)
- [ ] Define Scale 4 (e.g., Hardness)
- [ ] Define Scale 5 (e.g., Stabilizer)

Look for the TODO comments in the `InitializeScales()` method.
