namespace HotelPricingEngine.Models
{
    public class RoomAllocationRequest
    {
        public string RoomType { get; set; } // Type of the room (e.g., Deluxe, Standard, Suite)
        public int Nights { get; set; } // Number of nights for the stay
        public string Season { get; set; } // Season during which the booking is made (e.g., Peak Season, Off-Season)
        public SpecialRequests SpecialRequests { get; set; } // Special requests for the booking
    }
}
