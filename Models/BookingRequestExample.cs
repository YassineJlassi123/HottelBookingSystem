using HotelPricingEngine.Models;
using Swashbuckle.AspNetCore.Filters;
using static HotelPricingEngine.Models.Enums;

public class BookingRequestExample : IExamplesProvider<List<BookingRequest>>
{
    public List<BookingRequest> GetExamples()
    {
        return new List<BookingRequest>
        {
            new BookingRequest
            {
                RoomType = RoomType.Deluxe,
                Nights = 3,
                Season = Season.PeakSeason,
                SpecialRequests = new SpecialRequests
                {
                    ConnectingRoom = false,
                    PreferredView = PreferedView.sea
                },
                OccupancyRate = 80
            },
            new BookingRequest
            {
                RoomType = RoomType.Standard,
                Nights = 2,
                Season = Season.OffSeason,
                SpecialRequests = new SpecialRequests
                {
                    ConnectingRoom = true,
                    PreferredView = PreferedView.garden
                },
                OccupancyRate = 60
            }
        };
    }
    }