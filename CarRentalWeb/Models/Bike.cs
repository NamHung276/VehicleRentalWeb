using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models
{
    public class Bike : Vehicle
    {
        [Display(Name = "Electric Bike")]
        public bool IsElectric { get; set; }

        public override decimal CalculateCost(int days)
        {
            decimal costPerDay = IsElectric ? RatePerDay * 1.2m : RatePerDay;
            return costPerDay * days;
        }
    }
}
