using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models
{
    public abstract class Vehicle
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100)]
        public int Year { get; set; }

        [Required, StringLength(30)]
        public string Color { get; set; } = string.Empty;

        [Display(Name = "Supplier / Source")]
        [StringLength(50)]
        public string Supplier { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Rate Per Day ($)")]
        [Range(0, 10000)]
        public decimal RatePerDay { get; set; }

        [Display(Name = "Image Path")]
        public string ImagePath { get; set; } = "/images/default_car.jpg";

        // Polymorphic cost calculation
        public abstract decimal CalculateCost(int days);
    }
}
