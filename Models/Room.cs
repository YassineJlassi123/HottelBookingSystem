namespace HotelPricingEngine.Models
{
    public class Room
    {
        public int Id { get; set; } // Unique identifier for the room
        public bool HasConnectingRooms { get; set; } // Indicates if the room can be connected with other rooms
        public string AvailableView { get; set; } // Single available view for the room
        public bool IsAvailable { get; set; } // Indicates if the room is available for booking
    }
}
