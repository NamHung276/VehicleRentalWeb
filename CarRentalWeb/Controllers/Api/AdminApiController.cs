using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleRentalWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleRentalWeb.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminApiController : ControllerBase
    {
        private readonly RentalContext _context;
        public AdminApiController(RentalContext context)
        {
            _context = context;
        }

        // GET: api/admin/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalVehicles = await _context.Vehicles.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();
            var totalRentals = await _context.Rentals.CountAsync();

            // Count active/ongoing rentals — choose interpretation (Approved = ongoing)
            var activeRentals = await _context.Rentals.CountAsync(r => r.Status == RentalStatus.Approved);

            return Ok(new
            {
                TotalVehicles = totalVehicles,
                TotalCustomers = totalCustomers,
                TotalRentals = totalRentals,
                ActiveRentals = activeRentals
            });
        }
    }
}
