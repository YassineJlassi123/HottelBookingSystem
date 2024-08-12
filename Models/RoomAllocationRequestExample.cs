using HotelPricingEngine.Models;
using Swashbuckle.AspNetCore.Filters;
using static HotelPricingEngine.Models.Enums;

public class RoomAllocationRequestExample : IExamplesProvider<RoomAllocationRequest>
{
    public RoomAllocationRequest GetExamples()
    {
        return new RoomAllocationRequest
        {
            RoomType = RoomType.Deluxe,
            Season = Season.PeakSeason,
            SpecialRequests = new SpecialRequests
            {
                ConnectingRoom = false,
                PreferredView = PreferedView.sea
            },
            Nights = 3
        };
    }
}

public class RoomAllocationResponseExample : IExamplesProvider<RoomAllocationResponse>
{
    public RoomAllocationResponse GetExamples()
    {
        return new RoomAllocationResponse
        {
            AllocatedRoomId = 101, // Example value
            RoomType = RoomType.Deluxe,
            TotalPrice = 450.00, // Example value
            SpecialRequests = new SpecialRequests  { PreferredView= PreferedView.sea, ConnectingRoom = false }
        };
    }
}
