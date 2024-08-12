using System.ComponentModel.DataAnnotations;
using static HotelPricingEngine.Models.Enums;

namespace HotelPricingEngine.Models
{
    public class SpecialRequests
    {
        [Required]
        public PreferedView PreferredView { get; set; } // Preferred view (e.g., sea, garden, city)
        [Required]
        public bool ConnectingRoom { get; set; } // Whether a connecting room is preferred
    }
}
