using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models.ViewModels
{
    public class EditProfileViewModel
    {
        [Required]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
    }
}
