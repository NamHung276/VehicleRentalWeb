using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models
{
    public class Car : Vehicle
    {
        [Range(1, 10)]
        public int Seats { get; set; } = 4;

        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; } = "Regular";

        public override decimal CalculateCost(int days)
        {
            decimal baseCost = RatePerDay * days;
            if (FuelType.Equals("Premium", StringComparison.OrdinalIgnoreCase))
                baseCost *= 1.1m;
            return baseCost;
        }
    }
}
