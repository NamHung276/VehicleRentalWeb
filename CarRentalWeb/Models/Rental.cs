using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRentalWeb.Models
{
    public class Rental
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [Required]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Total Cost ($)")]
        [Range(0, 999999)]
        public decimal TotalCost { get; set; }

        // 🧮 Calculates total cost automatically
        public void CalculateTotalCost()
        {
            if (Vehicle != null && EndDate > StartDate)
            {
                int days = (EndDate - StartDate).Days;
                TotalCost = Vehicle.CalculateCost(days);
            }
        }
    }
}
