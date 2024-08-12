using CsvHelper;
using CsvHelper.Configuration;
using HotelPricingEngine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations; // Add Swagger Annotations
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using static HotelPricingEngine.Models.Enums;
using Swashbuckle.AspNetCore.Filters;

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
    [HttpGet("competitors")]
    [SwaggerOperation(
    Summary = "Get Competitor Names",
    Description = "Fetches the list of available competitor names."
)]
    [SwaggerResponse(200, "Returns the list of competitor names.", typeof(List<string>))]
    [SwaggerResponse(500, "If there is an error fetching competitor names.")]
    public async Task<ActionResult<List<string>>> GetCompetitorNames()
    {
        List<string> competitorNames;
        try
        {
            // Fetch competitor names
            competitorNames = await _pricingService.GetCompetitorNamesAsync();
        }
        catch (Exception ex)
        {
            // Log error and return a server error response
            _logger.LogError($"Error fetching competitor names: {ex.Message}");
            return StatusCode(500, "Error fetching competitor names");
        }

        return Ok(competitorNames);
    }

    /// <summary>
    /// Calculates the adjusted room price based on the provided pricing request.
    /// </summary>
    /// <param name="roomType">The type of room (e.g., Deluxe, Standard).</param>
    /// <param name="season">The season (e.g., Peak Season, Off-Season).</param>
    /// <param name="occupancyRate">The occupancy rate as a percentage.</param>
    /// <param name="competitorPrices">A comma-separated list of competitor names.</param>
    /// <returns>Returns the adjusted price for the room.</returns>
    /// <response code="200">Returns the adjusted price for the room.</response>
    /// <response code="400">If the request is invalid or contains errors.</response>
    /// <response code="500">If there is an error fetching competitor prices.</response>
    [HttpPost("calculate-price")]
    [SwaggerOperation(
        Summary = "Calculates Adjusted Room Price",
        Description = "Calculates the adjusted room price based on the provided parameters and competitor prices."
    )]
    [SwaggerResponse(200, "Returns the adjusted price for the room.", typeof(PricingResponse))]
    [SwaggerResponse(400, "If the request is invalid or contains errors.")]
    [SwaggerResponse(500, "If there is an error fetching competitor prices.")]
    [SwaggerRequestExample(typeof(PricingRequest), typeof(PricingRequestExample))]

    public async Task<ActionResult<PricingResponse>> CalculatePrice(
        [FromQuery, SwaggerParameter("The type of room (e.g., Deluxe, Standard).", Required = true)]
        Enums.RoomType roomType,

        [FromQuery, SwaggerParameter("The season (e.g., Peak Season, Off-Season).", Required = true)]
        Enums.Season season,

        [FromQuery, SwaggerParameter("The occupancy rate as a percentage.", Required = true)]
        int occupancyRate,

        [FromQuery, SwaggerParameter("A comma-separated list of competitor names.", Required = true)]
        string[] competitorPrices)  // Swagger UI handles arrays as comma-separated values
    {
        List<CompetitorPriceRecord> competitorPriceRecords;
        try
        {
            // Fetch competitor prices
            competitorPriceRecords = await _pricingService.GetCompetitorPricesAsync();
        }
        catch (Exception ex)
        {
            // Log error and return a server error response
            _logger.LogError($"Error fetching competitor prices: {ex.Message}");
            return StatusCode(500, "Error fetching competitor prices");
        }

        var request = new PricingRequest
        {
            RoomType = roomType,
            Season = season,
            OccupancyRate = occupancyRate,
            CompetitorPrices = competitorPrices.ToList()
        };

        // Calculate adjustment based on competitor prices
        double competitorAdjustment = _pricingService.GetCompetitorAdjustment(competitorPriceRecords, request);
        var response = _pricingService.CalculateAdjustedPrice(request, competitorAdjustment);

        // Return the adjusted price response
        return Ok(response);
    }
}
