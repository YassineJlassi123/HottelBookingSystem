﻿using HotelPricingEngine.Models;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;


namespace HotelPricingEngine.HotelPricingEngine.Tests
{
    public class RoomAllocationServiceTests
    {
        private readonly RoomAllocationService _service;
        private readonly Mock<ILogger<RoomAllocationService>> _mockLogger;

        public RoomAllocationServiceTests()
        {
            _mockLogger = new Mock<ILogger<RoomAllocationService>>();
            _service = new RoomAllocationService(_mockLogger.Object);
        }

        [Fact]
        public void AllocateRoom_ValidRequest_ReturnsCorrectResponse()
        {
            // Arrange
            var request = new RoomAllocationRequest
            {
                RoomType = "Deluxe",
                SpecialRequests = new SpecialRequests
                {
                    ConnectingRoom = false,
                    PreferredView = "sea"
                },
                Nights = 2
            };
            var pricingResponse = new PricingResponse
            {
                RoomType = "Deluxe",
                AdjustedPrice = 100
            };

            // Act
            var result = _service.AllocateRoom(request, pricingResponse);

            // Assert
            Assert.Equal(101, result.AllocatedRoomId);
            Assert.Equal(200, result.TotalPrice); // 100 price * 2 nights
        }

        [Fact]
        public void AllocateRoom_WhenRoomUnavailable_ReturnsNoRoomAvailable()
        {
            // Arrange
            var request = new RoomAllocationRequest
            {
                RoomType = "Deluxe",
                SpecialRequests = new SpecialRequests
                {
                    ConnectingRoom = false,
                    PreferredView = "sea"
                },
                Nights = 2
            };
            var pricingResponse = new PricingResponse
            {
                RoomType = "Deluxe",
                AdjustedPrice = 100
            };

            // Allocate room once
            _service.AllocateRoom(request, pricingResponse);

            // Act
            var result = _service.AllocateRoom(request, pricingResponse);

            // Assert
            Assert.Equal(-1, result.AllocatedRoomId);
        }

        [Fact]
        public void AllocateRoomWhenBooking_ValidRequestWithFactors_ReturnsCorrectResponse()
        {
            // Arrange
            var request = new BookingRequest
            {
                RoomType = "Deluxe",
                SpecialRequests = new SpecialRequests
                {
                    ConnectingRoom = false,
                    PreferredView = "sea"
                },
                Nights = 2
            };
            var pricingResponse = new PricingResponse
            {
                RoomType = "Deluxe",
                AdjustedPrice = 100
            };

            // Act
            var result = _service.AllocateRoomWhenBooking(request, pricingResponse);

            // Assert
            Assert.Equal(101, result.AllocatedRoomId);
            Assert.Equal(210.00, result.TotalPrice); // 100 price * 2 nights * (1 + 0.05)
        }

        [Fact]
        public void ResetRoomAvailability_AllRoomsAvailable()
        {
            // Arrange
            var request = new RoomAllocationRequest
            {
                RoomType = "Deluxe",
                SpecialRequests = new SpecialRequests
                {
                    ConnectingRoom = false,
                    PreferredView = "sea"
                },
                Nights = 2
            };
            var pricingResponse = new PricingResponse
            {
                RoomType = "Deluxe",
                AdjustedPrice = 100
            };
            _service.AllocateRoom(request, pricingResponse); // Allocate a room

            // Act
            _service.ResetRoomAvailability();

            // Assert
            var response = _service.AllocateRoom(request, pricingResponse);
            Assert.Equal(101, response.AllocatedRoomId); // Room should be available again
        }

    }
}
