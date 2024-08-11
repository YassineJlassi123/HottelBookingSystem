using HotelPricingEngine.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HotelPricingEngine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly PricingService _pricingService;
        private readonly RoomAllocationService _roomAllocationService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(RoomAllocationService roomAllocationService, PricingService pricingService, ILogger<BookingController> logger)
        {
            _roomAllocationService = roomAllocationService;
            _pricingService = pricingService;
            _logger = logger;
        }

        /// <summary>
        /// Books rooms based on the provided booking requests.
        /// </summary>
        /// <param name="bookingRequests">A list of booking requests containing room type, season, and occupancy rate.</param>
        /// <returns>Returns a list of room allocation responses, including allocated room IDs and pricing information.</returns>
        /// <response code="200">Returns the list of room allocations.</response>
        /// <response code="400">If no rooms are available for the specified request.</response>
        /// <response code="500">If there is an error fetching competitor prices.</response>
        [HttpPost("booking-rooms")]
        [SwaggerOperation(Summary = "Books rooms", Description = "Book multiple rooms based on the provided booking requests. \n" +
             " \n Notes: The type of room Must be one of: Standard, Deluxe, Suite.\n  " +
             "\n Notes : The occupancy rate as a percentage. Must be between 0 and 100.\n   " +
        "\n please enter the same syntax like described in the notes !! \n" +
        "\n Notes : The season during which the price is calculated. Must be one of: Peak Season, Off-Season.\n " +
          "\n Notes : for now we only have 3 rooms :  \n " +
           "\n  room1: Deluxe ; Id = 101 ; HasConnectingRooms = false ; AvailableView = sea   \n" +
              "\n room2: Standard ; Id = 202 ; HasConnectingRooms = true ; AvailableView = garden  \n"
            + "\n room3: Suite ; Id = 301 ; HasConnectingRooms = false ; AvailableView = city  ")]
        [SwaggerResponse(200, "Returns the list of room allocations.", typeof(List<RoomAllocationResponse>))]
        [SwaggerResponse(400, "If no rooms are available for the specified request.")]
        [SwaggerResponse(500, "If there is an error fetching competitor prices.")]
        public async Task<ActionResult<List<RoomAllocationResponse>>> BookRooms(
            [FromBody, SwaggerParameter("A list of booking requests containing room type, season, and occupancy rate.", Required = true)]
            List<BookingRequest> bookingRequests)
        {
            var allocationResponses = new List<RoomAllocationResponse>();

            foreach (var request in bookingRequests)
            {
                // Create a pricing request for each booking request
                PricingRequest pricingRequest = new PricingRequest
                {
                    RoomType = request.RoomType,
                    Season = request.Season,
                    OccupancyRate = request.OccupancyRate,
                };

                List<CompetitorPriceRecord> competitorPrices;
                try
                {
                    competitorPrices = await _pricingService.GetCompetitorPricesAsync(); // Fetch competitor prices
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error fetching competitor prices: {ex.Message}"); // Log error and return a server error response
                    return StatusCode(500, "Error fetching competitor prices");
                }

                // Calculate adjusted price
                double competitorAdjustment = _pricingService.GetCompetitorAdjustmentAutamatcly(competitorPrices, pricingRequest);
                var pricingResponse = _pricingService.CalculateAdjustedPrice(pricingRequest, competitorAdjustment);

                // Allocate room
                var allocationResponse = _roomAllocationService.AllocateRoomWhenBooking(request, pricingResponse);

                if (allocationResponse.AllocatedRoomId == -1)
                {
                    // Return a bad request response if no room is available
                    return BadRequest(new { message = $"Room for {request.RoomType} is not available." });
                }

                allocationResponses.Add(allocationResponse); // Add the allocation response to the list
            }

            return Ok(new { allocations = allocationResponses }); // Return the list of room allocations
        }
    }
}
