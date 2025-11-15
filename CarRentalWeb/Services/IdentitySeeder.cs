using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.ViewModels
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required]
        public string DisplayName { get; set; } = "";
    }
}
