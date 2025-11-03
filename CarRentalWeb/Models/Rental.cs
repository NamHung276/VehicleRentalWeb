using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRentalWeb.Models
{
    public class Rental
    {
        public int Id { get; set; }

        [Required, Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required, Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; } = string.Empty;

        [Required, DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Range(0, 10000)]
        [Display(Name = "Total Cost ($)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }
}
