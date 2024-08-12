using HotelPricingEngine.Models;
using static HotelPricingEngine.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

public class RoomAllocationService
{
    // Dictionary to hold room data, keyed by room type (using RoomType enum)
    private readonly Dictionary<RoomType, Room> _rooms = new Dictionary<RoomType, Room>
    {
        { RoomType.Deluxe, new Room { Id = 101, HasConnectingRooms = false, AvailableView = "sea", IsAvailable = true } },
        { RoomType.Standard, new Room { Id = 202, HasConnectingRooms = true, AvailableView = "garden", IsAvailable = true } },
        { RoomType.Suite, new Room { Id = 301, HasConnectingRooms = false, AvailableView = "city", IsAvailable = true } }
    };

    // Preferred view factors for adjusting prices
    private readonly Dictionary<PreferedView, double> _preferredViewFactors = new Dictionary<PreferedView, double>
    {
        { PreferedView.sea, 0.05 },
        { PreferedView.garden, 0.02 },
        { PreferedView.city, 0.03 }
    };

    // Connecting rooms availability factors
    private readonly Dictionary<bool, double> _connectingRoomsAvailableFactors = new Dictionary<bool, double>
    {
        { true, 0.05 },
        { false, 0 }
    };

    // Lock object to manage concurrent access to room availability
    private readonly object _lock = new object();
    private readonly ILogger<RoomAllocationService> _logger;

    // Constructor to inject the logger for logging service activities
    public RoomAllocationService(ILogger<RoomAllocationService> logger)
    {
        _logger = logger;
    }

    // Method to allocate a room based on booking request and pricing response
    public RoomAllocationResponse AllocateRoom(RoomAllocationRequest request, PricingResponse pricingResponse)
    {
        // Lock to ensure thread-safe access to the rooms dictionary
        lock (_lock)
        {
            // Try to get the room from the dictionary based on the room type from the request
            if (_rooms.TryGetValue(request.RoomType, out var room))
            {
                // Check if the room is available and meets the special request criteria
                if (room.IsAvailable &&
                    room.HasConnectingRooms == request.SpecialRequests.ConnectingRoom &&
                    room.AvailableView == request.SpecialRequests.PreferredView.ToString())
                {
                    // Calculate price adjustments based on preferred view and connecting rooms
                    double preferredViewFactor = _preferredViewFactors.GetValueOrDefault(request.SpecialRequests.PreferredView, 0);
                    double connectingRoomsAvailableFactor = _connectingRoomsAvailableFactors.GetValueOrDefault(request.SpecialRequests.ConnectingRoom, 0);

                    // Mark the room as unavailable since it's being allocated
                    room.IsAvailable = false;
                    _logger.LogInformation($"Allocating Room {room.Id} for {request.RoomType}. Marking as unavailable.");

                    // Calculate the total price
                    double totalPrice = Math.Round(pricingResponse.AdjustedPrice * request.Nights * (1 + preferredViewFactor + connectingRoomsAvailableFactor), 2);

                    // Return a successful room allocation response
                    return new RoomAllocationResponse
                    {
                        RoomType = request.RoomType,
                        AllocatedRoomId = room.Id,
                        TotalPrice = totalPrice,
                        SpecialRequests = request.SpecialRequests
                    };
                }
                else
                {
                    // Log a warning if the room does not match the special request criteria
                    _logger.LogWarning($"Room {room.Id} for {request.RoomType} does not match special requests.");
                }
            }
            else
            {
                // Log a warning if the room type was not found in the dictionary
                _logger.LogWarning($"Room type {request.RoomType} not found.");
            }

            // Return a response indicating no room could be allocated
            return new RoomAllocationResponse
            {
                RoomType = request.RoomType,
                AllocatedRoomId = -1, // Indicates no room available
                TotalPrice = 0,
                SpecialRequests = request.SpecialRequests
            };
        }
    }

    // Method to allocate a room based on BookingRequest and PricingResponse (for booking scenarios)
    public RoomAllocationResponse AllocateRoomWhenBooking(RoomAllocationRequest request, PricingResponse pricingResponse)
    {
        // Lock to ensure thread-safe access to the rooms dictionary
        lock (_lock)
        {
            // Try to get the room from the dictionary based on the room type from the request
            if (_rooms.TryGetValue(request.RoomType, out var room))
            {
                // Check if the room is available and meets the special request criteria
                if (room.IsAvailable &&
                    room.HasConnectingRooms == request.SpecialRequests.ConnectingRoom &&
                    room.AvailableView == request.SpecialRequests.PreferredView.ToString())
                {
                    // Calculate price adjustments based on preferred view and connecting rooms
                    double preferredViewFactor = _preferredViewFactors.GetValueOrDefault(request.SpecialRequests.PreferredView, 0);
                    double connectingRoomsAvailableFactor = _connectingRoomsAvailableFactors.GetValueOrDefault(request.SpecialRequests.ConnectingRoom, 0);

                    // Mark the room as unavailable since it's being allocated
                    room.IsAvailable = false;
                    _logger.LogInformation($"Allocating Room {room.Id} for {request.RoomType}. Marking as unavailable.");

                    // Calculate the total price
                    double totalPrice = Math.Round(pricingResponse.AdjustedPrice * request.Nights * (1 + preferredViewFactor + connectingRoomsAvailableFactor), 2);

                    // Return a successful room allocation response
                    return new RoomAllocationResponse
                    {
                        RoomType = request.RoomType,
                        AllocatedRoomId = room.Id,
                        TotalPrice = totalPrice,
                        SpecialRequests = request.SpecialRequests
                    };
                }
                else
                {
                    // Log a warning if the room does not match the special request criteria
                    _logger.LogWarning($"Room {room.Id} for {request.RoomType} does not match special requests.");
                }
            }
            else
            {
                // Log a warning if the room type was not found in the dictionary
                _logger.LogWarning($"Room type {request.RoomType} not found.");
            }

            // Return a response indicating no room could be allocated
            return new RoomAllocationResponse
            {
                RoomType = request.RoomType,
                AllocatedRoomId = -1, // Indicates no room available
                TotalPrice = 0,
                SpecialRequests = request.SpecialRequests
            };
        }
    }

    // Method to reset room availability to true for all rooms
    public void ResetRoomAvailability()
    {
        // Lock to ensure thread-safe access while resetting room availability
        lock (_lock)
        {
            foreach (var room in _rooms.Values)
            {
                room.IsAvailable = true;
            }
            _logger.LogInformation("Room availability has been reset.");
        }
    }
}
