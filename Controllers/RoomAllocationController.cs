using HotelPricingEngine.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HotelPricingEngine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomAllocationController : ControllerBase
    {
        private readonly RoomAllocationService _roomAllocationService;
        private readonly PricingService _pricingService;
        private readonly ILogger<RoomAllocationController> _logger;

        public RoomAllocationController(RoomAllocationService roomAllocationService, PricingService pricingService, ILogger<RoomAllocationController> logger)
        {
            _roomAllocationService = roomAllocationService;
            _pricingService = pricingService;
            _logger = logger;
        }

        /// <summary>
        /// Books multiple rooms based on booking requests.
        /// </summary>
        /// <param name="roomAllocationRequest">The booking request details.</param>
        /// <returns>A response containing the allocated room details.</returns>
        [HttpPost("book-room")]
        [SwaggerOperation(Summary = "Book a room", Description = "Book room based on the provided booking request. \n" +
           " \n Notes: The type of room Must be one of: Standard, Deluxe, Suite.\n  " +
        "\n Notes : The season during which the price is calculated. Must be one of: Peak Season, Off-Season.\n " +
          "\n Notes : for now we only have 3 rooms :  \n " +
           "\n  room1: Deluxe ; Id = 101 ; HasConnectingRooms = false ; AvailableView = sea   \n" +
              "\n room2: Standard ; Id = 202 ; HasConnectingRooms = true ; AvailableView = garden  \n"
            + "\n room3: Suite ; Id = 301 ; HasConnectingRooms = false ; AvailableView = city  ")]
        [SwaggerResponse(200, "Room allocation successful", typeof(RoomAllocationResponse))]
        [SwaggerResponse(400, "Room not available")]
        [SwaggerResponse(500, "Error fetching competitor prices")]
        public async Task<ActionResult<RoomAllocationResponse>> BookRooms([FromBody] RoomAllocationRequest roomAllocationRequest)
        {
            var roomAllocationResponses = new RoomAllocationResponse();

            // Create a pricing request for each booking request
            PricingRequest pricingRequest = new PricingRequest
            {
                RoomType = roomAllocationRequest.RoomType,
                Season = roomAllocationRequest.Season,
                OccupancyRate = 40
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
            var allocationResponse = _roomAllocationService.AllocateRoom(roomAllocationRequest, pricingResponse);
            Console.WriteLine(allocationResponse.AllocatedRoomId);
            if (allocationResponse.AllocatedRoomId == -1)
            {
                // Return a bad request response if no room is available
                return BadRequest(new { message = $"Room for {roomAllocationRequest.RoomType} is not available." });
            }

            return Ok(allocationResponse); // Return room allocations
        }

        /// <summary>
        /// Resets room availability to available.
        /// </summary>
        /// <returns>A success message indicating the reset was successful.</returns>
        [HttpPost("reset")]
        [SwaggerOperation(Summary = "Reset room availability", Description = "Resets all rooms to available.")]
        [SwaggerResponse(200, "Room availability has been reset.")]
        public IActionResult ResetRoomAvailability()
        {
            _roomAllocationService.ResetRoomAvailability(); // Reset all rooms to available
            return Ok("Room availability has been reset."); // Return success message
        }
    }
}