using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRentalWeb.Models
{
    public enum RentalStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class Rental
    {
        public int Id { get; set; }

        // FK - Customer
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // FK - Vehicle
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Rental Fee ($)")]
        [Range(0, 100000)]
        public decimal RentalFee { get; set; }

        // *** RESTORED FIELDS ***
        public bool IsActive { get; set; } = false;

        public RegistrationType Source { get; set; } = RegistrationType.Online;

        [NotMapped]
        public decimal TotalCost =>
            Vehicle != null
                ? Vehicle.RatePerDay * (int)(EndDate - StartDate).TotalDays
                : RentalFee;

        [Display(Name = "Status")]
        public RentalStatus Status { get; set; } = RentalStatus.Pending;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
