using static HotelPricingEngine.Models.Enums;
using System.ComponentModel.DataAnnotations;
using Xunit.Sdk;
using HotelPricingEngine.Models;

public class PricingRequest
{
    [Required]
    
    public RoomType RoomType { get; set; }

    [Required]
 

    public Season Season { get; set; }

    [Required]
    [Range(0, 100, ErrorMessage = "Occupancy rate must be between 0 and 100.")]
    public int OccupancyRate { get; set; }

    public List<string> CompetitorPrices { get; set; }
}