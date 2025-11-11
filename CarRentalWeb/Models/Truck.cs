using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models
{
    public class Truck : Vehicle
    {
        [Display(Name = "Capacity (Tons)")]
        [Range(0.5, 50)]
        public double CapacityInTons { get; set; } = 2.0;

        [Display(Name = "Transmission")]
        public string Transmission { get; set; } = "Manual";

        [Display(Name = "Air Conditioning")]
        public bool HasAC { get; set; } = true;

        public override decimal CalculateCost(int days)
        {
            return RatePerDay * days + (decimal)(CapacityInTons * 20);
        }
    }
}
