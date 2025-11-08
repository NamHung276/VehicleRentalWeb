using System.ComponentModel.DataAnnotations;

namespace VehicleRentalWeb.Models
{
    public enum RentalSource
    {
        [Display(Name = "Online (Website)")]
        Online,

        [Display(Name = "Offline (In-person)")]
        Offline
    }

    public enum RegistrationType
    {
        [Display(Name = "Online Registration")]
        Online,

        [Display(Name = "Walk-In Registration")]
        WalkIn
    }
}
