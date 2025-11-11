using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models
{
    public class Car : Vehicle
    {
        [Range(1, 10)]
        [Display(Name = "Seats")]
        public int Seats { get; set; } = 5;

        [Display(Name = "Luggage Capacity")]
        public string Luggage { get; set; } = "1 Large, 1 Small";

        [Display(Name = "Transmission")]
        public string Transmission { get; set; } = "Automatic";

        [Display(Name = "Air Conditioning")]
        public bool HasAC { get; set; } = true;

        [Display(Name = "CarPlay / Android Auto")]
        public bool HasCarPlay { get; set; } = true;

        [Display(Name = "Mileage Policy")]
        public string MileagePolicy { get; set; } = "Unlimited";

        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; } = "Fuel";

        public override decimal CalculateCost(int days)
        {
            decimal baseCost = RatePerDay * days;
            if (FuelType.Equals("Electric", StringComparison.OrdinalIgnoreCase))
                baseCost *= 1.1m;
            return baseCost;
        }
    }
}
