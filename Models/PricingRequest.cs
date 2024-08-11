namespace HotelPricingEngine.Models
{
    public class PricingRequest
    {
        public string RoomType { get; set; }
        public string Season { get; set; }
        public int OccupancyRate { get; set; }
        public List<string> CompetitorPrices { get; set; }
    }
}
