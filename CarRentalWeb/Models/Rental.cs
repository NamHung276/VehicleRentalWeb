using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRentalWeb.Models
{
    public class Rental
    {
        public int Id { get; set; }

        // Foreign keys
        [Required]
        public int CustomerId { get; set; }
        [Required]
        public int VehicleId { get; set; }

        // Navigation properties
        public Customer Customer { get; set; } = null!;  // EF will populate
        public Vehicle Vehicle { get; set; } = null!;    // EF will populate

        // Rental period
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Total cost, writable for calculation
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        // Statuses
        [Required]
        public RentalStatus Status { get; set; } = RentalStatus.Pending;

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        // Payment info
        public DateTime? PaymentDate { get; set; }
        public string? TransactionId { get; set; }  // null until payment

        // Audit
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Computed property
        [NotMapped]
        public bool IsActive => Status == RentalStatus.Pending || Status == RentalStatus.Approved;
    }
}
