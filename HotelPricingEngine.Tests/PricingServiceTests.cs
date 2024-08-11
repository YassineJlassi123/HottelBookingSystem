using HotelPricingEngine.Models;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

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
                RoomType = "Deluxe",
                Season = "Peak Season",
                OccupancyRate = 50
            };
            var competitorAdjustment = 0.05;

            // Act
            var result = _pricingService.CalculateAdjustedPrice(request, competitorAdjustment);

            // Assert
            Assert.Equal(135.00, result.AdjustedPrice);
            Assert.Equal("Deluxe", result.RoomType);
        }

        [Fact]
        public void CalculateAdjustedPrice_InvalidRoomType_ThrowsException()
        {
            // Arrange
            var request = new PricingRequest
            {
                RoomType = "Invalid",
                Season = "Off-Season",
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
            new CompetitorPriceRecord { RoomType = "Deluxe", Season = "Peak Season", BasePrice = 120 ,OccupancyRate = 50,Competitor="Example" },
            new CompetitorPriceRecord { RoomType = "Deluxe", Season = "Peak Season", BasePrice = 130 ,OccupancyRate = 50,Competitor="Example" }
        };
            var request = new PricingRequest { RoomType = "Deluxe", Season = "Peak Season", OccupancyRate = 50, CompetitorPrices = ["Example"] };

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
            var request = new PricingRequest { RoomType = "Deluxe", Season = "Peak Season" };

            // Act
            var result = _pricingService.GetCompetitorAdjustment(competitorPrices, request);

            // Assert
            Assert.Equal(0, result);
        }

    }
    }
