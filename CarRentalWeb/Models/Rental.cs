using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRentalWeb.Models
{
    public class Rental
    {
        public int Id { get; set; }

        // Foreign Key: Customer (nullable)
        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        // Foreign Key: Vehicle (nullable)
        [Display(Name = "Vehicle")]
        public int? VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Range(0, 10000)]
        [Display(Name = "Total Cost ($)")]
        [Column(TypeName = "decimal(18,2)")] // Fix precision warning
        public decimal TotalCost { get; set; }

        [Display(Name = "Rental Source")]
        public RentalSource Source { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Archived")]
        public bool IsArchived { get; set; } = false;

    }
}
