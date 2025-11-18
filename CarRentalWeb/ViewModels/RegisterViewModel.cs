using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.ViewModels
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = "";

        [Required]
        [Display(Name = "Full Name")]
        public string DisplayName { get; set; } = "";

        // NEW — Required Phone for Customer model
        [Required]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = "";

        // NEW — Optional address
        [Display(Name = "Address")]
        public string Address { get; set; } = "";
    }
}
