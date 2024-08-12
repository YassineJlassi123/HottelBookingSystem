using HotelPricingEngine.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static HotelPricingEngine.Models.Enums;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;

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
        /// <param name="roomType">The type of room to book.</param>
        /// <param name="season">The season during which the price is calculated.</param>
        /// <param name="nights">The number of nights to book.</param>
        /// <param name="connectingRoom">Whether connecting rooms are requested.</param>
        /// <param name="preferredView">The preferred view for the room.</param>
        /// <returns>A response containing the allocated room details.</returns>
        [HttpPost("book-room")]
        [SwaggerOperation(Summary = "Book a room", Description = "Book room based on the provided booking parameters. \n" +
           "\n Notes: For now, we only have 3 rooms:\n" +
           "\n  room1: Deluxe ; Id = 101 ; HasConnectingRooms = false ; AvailableView = sea\n" +
           "\n  room2: Standard ; Id = 202 ; HasConnectingRooms = true ; AvailableView = garden\n" +
           "\n  room3: Suite ; Id = 301 ; HasConnectingRooms = false ; AvailableView = city")]
        [SwaggerResponse(200, "Room allocation successful", typeof(RoomAllocationResponse))]
        [SwaggerResponse(400, "Room not available")]
        [SwaggerResponse(500, "Error fetching competitor prices")]
        [SwaggerRequestExample(typeof(RoomAllocationRequest), typeof(RoomAllocationRequestExample))]

        public async Task<ActionResult<RoomAllocationResponse>> BookRooms(
              [FromQuery, SwaggerParameter("The type of room (e.g., Deluxe, Standard).", Required = true)]
        Enums.RoomType roomType,
            [FromQuery, SwaggerParameter("The season (e.g., Peak Season, Off-Season).", Required = true)]
        Enums.Season season,
            [FromQuery, SwaggerParameter(Required = true)]  int nights,
            [FromQuery, SwaggerParameter("The connectingRooms (e.g.,true, false)..", Required = true)] bool connectingRoom,
            [FromQuery, SwaggerParameter("The PreferedView (e.g.,sea, garden).", Required = true)] PreferedView preferredView)
        {
            var roomAllocationRequest = new RoomAllocationRequest
            {
                RoomType = roomType,
                Season = season,
                SpecialRequests = new SpecialRequests
                {
                    ConnectingRoom = connectingRoom,
                    PreferredView = preferredView
                },
                Nights = nights
            };

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
