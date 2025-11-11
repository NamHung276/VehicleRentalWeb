using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models
{
    public class Bike : Vehicle
    {
        [Display(Name = "Engine (CC)")]
        [Range(50, 2000)]
        public int EngineCC { get; set; } = 150;

        [Display(Name = "Electric Bike")]
        public bool IsElectric { get; set; } = false;

        [Display(Name = "Has Storage Box")]
        public bool HasStorageBox { get; set; } = true;

        [Display(Name = "Transmission")]
        public string Transmission { get; set; } = "Manual";

        public override decimal CalculateCost(int days)
        {
            decimal rate = RatePerDay * days;
            if (IsElectric) rate *= 1.2m;
            return rate;
        }
    }
}
