namespace HotelPricingEngine.Models
{
    public class CompetitorPriceRecord
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public string RoomType { get; set; }
        public string Season { get; set; }
        public int OccupancyRate { get; set; }
        public double BasePrice { get; set; }
        public string Competitor { get; set; }
        public int ID { get; set; }
        public bool HaveConnectingRooms { get; set; }
        public string AvailableViews { get; set; }
    }
}
