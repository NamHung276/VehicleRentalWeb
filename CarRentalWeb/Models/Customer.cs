using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Profile Image")]
        public string ImagePath { get; set; } = "/images/default_customer.jpg";

        [Display(Name = "Registration Type")]
        public RegistrationType RegistrationType { get; set; }

        // This is the foreign key side
        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}
