using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }
}
