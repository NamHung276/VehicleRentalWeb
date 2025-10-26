using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models
{
    public class Truck : Vehicle
    {
        [Display(Name = "Capacity (Tons)")]
        [Range(0.5, 50)]
        public double CapacityInTons { get; set; }

        public override decimal CalculateCost(int days)
        {
            return RatePerDay * days + (decimal)(CapacityInTons * 20);
        }
    }
}
