using CsvHelper;
using CsvHelper.Configuration;
using HotelPricingEngine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations; // Add Swagger Annotations
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class PricingController : ControllerBase
{
    private readonly PricingService _pricingService;
    private readonly ILogger<PricingController> _logger;

    // Constructor to initialize services and logger
    public PricingController(PricingService pricingService, ILogger<PricingController> logger)
    {
        _pricingService = pricingService;
        _logger = logger;
    }

    /// <summary>
    /// Calculates the adjusted room price based on the provided pricing request and.
    /// </summary>
    /// <param name="request">The pricing request containing details such as room type, season, and occupancy rate.</param>
    /// <returns>Returns the adjusted price for the room.</returns>
    /// <response code="200">Returns the adjusted price for the room.</response>
    /// <response code="400">If the request is invalid or contains errors.</response>
    /// <response code="500">If there is an error fetching competitor prices.</response>
    [HttpPost("calculate-price")]
    [SwaggerOperation(
        Summary = "Calculates Adjusted Room Price",
        Description = "Calculates the adjusted room price based on the provided pricing request and competitor prices. \n" +
        "\n Notes : The type of room. Must be one of: Standard, Deluxe, Suite.\n  " +
        "\n Notes : The season during which the price is calculated. Must be one of: Peak Season, Off-Season.\n " +
        "\n Notes : The occupancy rate as a percentage. Must be between 0 and 100.\n   " +
        "\n please enter the same syntax like described in the notes !!"
    )]
    [SwaggerResponse(200, "Returns the adjusted price for the room.", typeof(PricingResponse))]
    [SwaggerResponse(400, "If the request is invalid or contains errors.")]
    [SwaggerResponse(500, "If there is an error fetching competitor prices.")]
    public async Task<ActionResult<PricingResponse>> CalculatePrice(
        [FromBody, SwaggerParameter("Pricing request containing room type, season, and occupancy rate.", Required = true)]
        PricingRequest request)
    {
        List<CompetitorPriceRecord> competitorPrices;
        try
        {
            // Fetch competitor prices
            competitorPrices = await _pricingService.GetCompetitorPricesAsync();
        }
        catch (Exception ex)
        {
            // Log error and return a server error response
            _logger.LogError($"Error fetching competitor prices: {ex.Message}");
            return StatusCode(500, "Error fetching competitor prices");
        }

        // Calculate adjustment based on competitor prices
        double competitorAdjustment = _pricingService.GetCompetitorAdjustment(competitorPrices, request);
        var response = _pricingService.CalculateAdjustedPrice(request, competitorAdjustment);

        // Return the adjusted price response
        return Ok(response);
    }
}
