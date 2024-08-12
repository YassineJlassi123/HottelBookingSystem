using static HotelPricingEngine.Models.Enums;

namespace HotelPricingEngine.Models
{
    public class PricingResponse

    {
        public RoomType RoomType { get; set; }
        public double BasePrice { get; set; }
        public double AdjustedPrice { get; set; }
    }
}
