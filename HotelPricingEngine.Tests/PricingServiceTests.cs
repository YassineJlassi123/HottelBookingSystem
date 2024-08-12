using HotelPricingEngine.Models;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using static HotelPricingEngine.Models.Enums;

namespace HotelPricingEngine.HotelPricingEngine.Tests
{
    public class PricingServiceTests
    {
        private readonly PricingService _pricingService;
        private readonly Mock<ILogger<PricingService>> _loggerMock;

        public PricingServiceTests()
        {
            _loggerMock = new Mock<ILogger<PricingService>>();
            _pricingService = new PricingService(_loggerMock.Object);
        }

        [Fact]
        public void CalculateAdjustedPrice_ValidRequest_ReturnsCorrectPrice()
        {
            // Arrange
            var request = new PricingRequest
            {
                RoomType = RoomType.Deluxe,  // Updated to use the enum
                Season = Season.PeakSeason,  // Updated to use the enum
                OccupancyRate = 50
            };
            var competitorAdjustment = 0.05;

            // Act
            var result = _pricingService.CalculateAdjustedPrice(request, competitorAdjustment);

            // Assert
            Assert.Equal(189.00, result.AdjustedPrice);
            Assert.Equal(RoomType.Deluxe, result.RoomType);  // Updated to use the enum
        }

        [Fact]
        public void CalculateAdjustedPrice_InvalidRoomType_ThrowsException()
        {
            // Arrange
            var request = new PricingRequest
            {
                RoomType = (RoomType)999, // Invalid enum value to simulate an invalid room type
                Season = Season.OffSeason,  // Updated to use the enum
                OccupancyRate = 50
            };
            var competitorAdjustment = 0.05;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _pricingService.CalculateAdjustedPrice(request, competitorAdjustment));
        }

        [Fact]
        public void GetCompetitorAdjustment_ValidCompetitorPrices_ReturnsCorrectAdjustment()
        {
            // Arrange
            var competitorPrices = new List<CompetitorPriceRecord>
            {
                new CompetitorPriceRecord { RoomType = "Deluxe", Season = "Peak Season", BasePrice = 140, OccupancyRate = 50, Competitor = "Example" },
                new CompetitorPriceRecord { RoomType = "Deluxe", Season = "Peak Season", BasePrice = 150, OccupancyRate = 50, Competitor = "Example" }
            };
            var request = new PricingRequest { RoomType = RoomType.Deluxe, Season = Season.PeakSeason, OccupancyRate = 50, CompetitorPrices = new List<string> { "Example" } };

            // Act
            var result = _pricingService.GetCompetitorAdjustment(competitorPrices, request);

            // Assert
            Assert.Equal(0.10, result); // (125 - 100) / 100 = 0.25 but clamped to 0.10
        }

        [Fact]
        public void GetCompetitorAdjustment_NoCompetitorPrices_ReturnsZeroAdjustment()
        {
            // Arrange
            var competitorPrices = new List<CompetitorPriceRecord>();
            var request = new PricingRequest { RoomType = RoomType.Deluxe, Season = Season.PeakSeason };

            // Act
            var result = _pricingService.GetCompetitorAdjustment(competitorPrices, request);

            // Assert
            Assert.Equal(0, result);
        }
    }
}
