using HotelPricingEngine.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static HotelPricingEngine.Models.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace HotelPricingEngine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly RoomAllocationService _roomAllocationService;
        private readonly PricingService _pricingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(RoomAllocationService roomAllocationService, PricingService pricingService, ILogger<BookingController> logger)
        {
            _roomAllocationService = roomAllocationService;
            _pricingService = pricingService;
            _logger = logger;
        }

        /// <summary>
        /// Books multiple rooms based on the request body parameters.
        /// </summary>
        /// <param name="bookingRequests">The list of booking details for multiple rooms.</param>
        /// <returns>A response containing the allocated room details.</returns>
        [HttpPost("book-rooms")]
        [SwaggerOperation(Summary = "Book multiple rooms", Description = "Book multiple rooms based on the provided booking parameters.")]
        [SwaggerResponse(200, "Room allocations successful", typeof(List<RoomAllocationResponse>))]
        [SwaggerResponse(400, "One or more rooms not available")]
        [SwaggerResponse(500, "Error fetching competitor prices")]
        [SwaggerRequestExample(typeof(List<BookingRequest>), typeof(BookingRequestExample))]
        public async Task<ActionResult<List<RoomAllocationResponse>>> BookRooms(
            [FromBody, SwaggerParameter("The list of booking details for multiple rooms.", Required = true)] List<BookingRequest> bookingRequests)
        {
            var roomAllocationResponses = new List<RoomAllocationResponse>();

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

            foreach (var bookingRequest in bookingRequests)
            {
                var roomAllocationRequest = new RoomAllocationRequest
                {
                    RoomType = bookingRequest.RoomType,
                    Season = bookingRequest.Season,
                    SpecialRequests = bookingRequest.SpecialRequests,
                    Nights = bookingRequest.Nights
                };

                PricingRequest pricingRequest = new PricingRequest
                {
                    RoomType = roomAllocationRequest.RoomType,
                    Season = roomAllocationRequest.Season,
                    OccupancyRate = bookingRequest.OccupancyRate // This value can be dynamic based on the scenario
                };

                // Calculate adjusted price
                double competitorAdjustment = _pricingService.GetCompetitorAdjustmentAutamatcly(competitorPrices, pricingRequest);
                var pricingResponse = _pricingService.CalculateAdjustedPrice(pricingRequest, competitorAdjustment);

                // Allocate room
                var allocationResponse = _roomAllocationService.AllocateRoom(roomAllocationRequest, pricingResponse);

                if (allocationResponse.AllocatedRoomId == -1)
                {
                    // Return a bad request response if any room is not available
                    return BadRequest(new { message = $"Room for {roomAllocationRequest.RoomType} is not available." });
                }

                roomAllocationResponses.Add(allocationResponse);
            }

            return Ok(roomAllocationResponses); // Return all room allocations
        }
    }
}
