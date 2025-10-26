using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required, Display(Name = "Full Name"), StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Driver License No."), StringLength(30)]
        public string LicenseNumber { get; set; } = string.Empty;
    }
}
