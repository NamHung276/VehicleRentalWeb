using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Customer"; // or "Admin"

        // Linked Customer
        public Customer? Customer { get; set; }
    }
}
