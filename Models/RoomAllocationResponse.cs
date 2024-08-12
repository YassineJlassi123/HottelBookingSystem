using static HotelPricingEngine.Models.Enums;

namespace HotelPricingEngine.Models
{
    public class RoomAllocationResponse
    {
        public RoomType RoomType { get; set; } // Type of the allocated room
        public int AllocatedRoomId { get; set; } // ID of the allocated room
        public double TotalPrice { get; set; } // Total price for the booking
        public SpecialRequests SpecialRequests { get; set; } // Special requests made for the booking
    }
}
