using HotelPricingEngine.Models;
using static HotelPricingEngine.Models.Enums;
using Swashbuckle.AspNetCore.Filters;

public class PricingRequestExample : IExamplesProvider<PricingRequest>
{
    public PricingRequest GetExamples()
    {
        return new PricingRequest
        {
            RoomType = RoomType.Deluxe,
            Season = Season.PeakSeason,
            OccupancyRate = 80,
            CompetitorPrices = ["Nazl Al Jabal"]
        };
    }
}

public class PricingResponseExample : IExamplesProvider<PricingResponse>
{
    public PricingResponse GetExamples()
    {
        return new PricingResponse
        {
            AdjustedPrice = 224.00 // Example value
        };
    }
}
