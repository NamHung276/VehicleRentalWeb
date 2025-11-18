using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models
{
    public abstract class Vehicle
    {
        public int Id { get; set; }

        [Required]
        public string Make { get; set; } = string.Empty;

        [Required]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100)]
        public int Year { get; set; }

        public string Color { get; set; } = string.Empty;

        public string Supplier { get; set; } = string.Empty;

        [Display(Name = "Rate Per Day ($)")]
        [Range(0, 10000)]
        public decimal RatePerDay { get; set; }

        public string ImagePath { get; set; } = "/images/default_car.jpg";

        // ✅ Shared universal filterable fields
        [Display(Name = "Category")]
        public string Category { get; set; } = "General"; // SUV, Sedan, etc.

        [Display(Name = "Status")]
        public string Status { get; set; } = "Available"; // Available, Rented, In Service

        public bool IsRemoved { get; set; } = false;

        // Derived convenience
        public bool IsAvailable => Status == "Available";

        // Each subclass must implement cost logic
        public abstract decimal CalculateCost(int days);
    }
}
