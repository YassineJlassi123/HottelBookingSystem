namespace HotelPricingEngine.Models
{
    public class BookingResponse
    {
        public bool Success { get; set; } // Indicates whether the booking was successful
        public string Message { get; set; } // Message providing additional information (e.g., errors or confirmations)
        public RoomAllocationResponse RoomDetails { get; set; } // Details of the allocated room, if booking was successful
    }
}
