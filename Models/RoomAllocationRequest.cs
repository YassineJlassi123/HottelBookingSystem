using System.ComponentModel.DataAnnotations;
using static HotelPricingEngine.Models.Enums;

namespace HotelPricingEngine.Models
{
    public class RoomAllocationRequest
    {
        [Required]
        public RoomType RoomType { get; set; } // Type of the room (e.g., Deluxe, Standard, Suite)
        [Required]
        public int Nights { get; set; } // Number of nights for the stay
        [Required]
        public Season Season { get; set; } // Season during which the booking is made (e.g., Peak Season, Off-Season)
        [Required]
        public SpecialRequests SpecialRequests { get; set; } // Special requests for the booking
    }
}
