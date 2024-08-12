using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;
namespace HotelPricingEngine.Models
{
    public class Enums
    {

       
        public enum RoomType
        {
            Standard,
            Deluxe,
            Suite
        }
      
        public enum Season
        {
            OffSeason,
            PeakSeason
        }
        public enum PreferedView
        {
            sea,
            garden,
            city
        }
    }
}
