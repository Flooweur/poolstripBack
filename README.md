# Color Comparison API

A .NET 8.0 Web API for comparing hex colors against predefined scales, similar to pool test strip analysis.

## Quick Start

```bash
cd ColorComparisonApi
dotnet run
```

Then visit: https://localhost:5001/swagger

## What This Does

This API accepts **5 hex color codes** via a POST request and compares each color against a corresponding scale (like pH, Chlorine, Alkalinity, Hardness, Stabilizer in pool testing). 

It uses **Euclidean distance in RGB color space** to find the closest matching reference color on each scale and returns the associated value.

## ⚠️ Important: Define Your Scales

Before using the API, you **must define the 5 color scales** in:

`ColorComparisonApi/Services/ColorComparisonService.cs`

Look for the **TODO comments** in the `InitializeScales()` method. You'll find 5 commented examples showing you exactly how to define each scale with:
- Scale name
- Color points (value + hex color pairs)

### Example Scale Definition

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

## API Endpoint

**POST** `/api/ColorComparison/compare`

**Request:**
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
    }
    // ... 4 more results
  ],
  "success": true,
  "message": null
}
```

## Testing

Use the included `.http` file with VS Code REST Client extension:
- `ColorComparisonApi/ColorComparisonApi.http`

Or use curl:
```bash
curl -X POST "https://localhost:5001/api/ColorComparison/compare" \
  -H "Content-Type: application/json" \
  -d @ColorComparisonApi/sample-request.json
```

## Documentation

See `ColorComparisonApi/README.md` for detailed documentation.
