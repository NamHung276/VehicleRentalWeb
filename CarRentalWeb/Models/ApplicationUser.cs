using Microsoft.AspNetCore.Identity;

namespace VehicleRentalWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Display name
        public string? DisplayName { get; set; }

        // Link to Customer if you want 1:1 later
        public int? CustomerId { get; set; }
    }
}
