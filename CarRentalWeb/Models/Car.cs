using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models
{
    public class Car : Vehicle
    {
        [Range(1, 10)]
        public int Seats { get; set; }

        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; } = "Regular";
        [Display(Name = "Image Path")]
        public new string ImagePath { get; set; } = "/images/default_car.jpg";

        public override decimal CalculateCost(int days)
        {
            decimal baseCost = RatePerDay * days;
            if (FuelType.Equals("Premium", StringComparison.OrdinalIgnoreCase))
                baseCost *= 1.1m; // 10% extra for premium fuel cars
            return baseCost;
        }
    }
}
