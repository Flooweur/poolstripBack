using ColorComparisonApi.Models;
using ColorComparisonApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ColorComparisonApi.Controllers;

/// <summary>
/// Controller for color comparison operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ColorComparisonController : ControllerBase
{
    private readonly IColorComparisonService _colorComparisonService;
    private readonly ILogger<ColorComparisonController> _logger;

    public ColorComparisonController(
        IColorComparisonService colorComparisonService,
        ILogger<ColorComparisonController> logger)
    {
        _colorComparisonService = colorComparisonService;
        _logger = logger;
    }

    /// <summary>
    /// Compares 5 hex color codes against predefined scales (like pool test strips)
    /// </summary>
    /// <param name="request">Request containing 5 hex color codes</param>
    /// <returns>Scale results with values for each color</returns>
    /// <response code="200">Returns the scale comparison results</response>
    /// <response code="400">If the input is invalid</response>
    [HttpPost("compare")]
    [ProducesResponseType(typeof(ColorComparisonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ColorComparisonResponse> CompareColors([FromBody] ColorComparisonRequest request)
    {
        try
        {
            _logger.LogInformation("Received color comparison request with {Count} colors", 
                request.ColorHexCodes?.Length ?? 0);

            if (request.ColorHexCodes == null)
            {
                return BadRequest(new ColorComparisonResponse
                {
                    Success = false,
                    Message = "ColorHexCodes cannot be null"
                });
            }

            var response = _colorComparisonService.CompareColors(request.ColorHexCodes);

            if (!response.Success)
            {
                _logger.LogWarning("Color comparison failed: {Message}", response.Message);
                return BadRequest(response);
            }

            _logger.LogInformation("Color comparison completed successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during color comparison");
            return BadRequest(new ColorComparisonResponse
            {
                Success = false,
                Message = $"An error occurred: {ex.Message}"
            });
        }
    }
}
