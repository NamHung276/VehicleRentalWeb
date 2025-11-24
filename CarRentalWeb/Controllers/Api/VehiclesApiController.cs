using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalWeb.Models;

namespace VehicleRentalWeb.Api
{
    [Route("api/vehicles")]
    [ApiController]
    public class VehiclesApiController : ControllerBase
    {
        private readonly RentalContext _context;

        public VehiclesApiController(RentalContext context)
        {
            _context = context;
        }

        // GET: api/vehicles
        [HttpGet]
        public async Task<IActionResult> GetVehicles()
        {
            var vehicles = await _context.Vehicles
                .Where(v => !v.IsRemoved)
                .ToListAsync();

            return Ok(vehicles);
        }

        // GET: api/vehicles/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null || vehicle.IsRemoved)
                return NotFound();

            return Ok(vehicle);
        }

        // POST: api/vehicles
        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] Vehicle vehicle)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Default new vehicles to Available
            vehicle.Status = "Available";
            vehicle.IsRemoved = false;

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicle);
        }

        // PUT: api/vehicles/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] Vehicle updated)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null || vehicle.IsRemoved)
                return NotFound();

            // Update allowed editable fields
            vehicle.Make = updated.Make;
            vehicle.Model = updated.Model;
            vehicle.Year = updated.Year;
            vehicle.Color = updated.Color;
            vehicle.Supplier = updated.Supplier;
            vehicle.RatePerDay = updated.RatePerDay;
            vehicle.ImagePath = updated.ImagePath;
            vehicle.Category = updated.Category;
            vehicle.Status = updated.Status; // "Available", "Rented", "In Service"

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/vehicles/5 (soft delete)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> ArchiveVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null || vehicle.IsRemoved)
                return NotFound();

            // Soft delete
            vehicle.IsRemoved = true;
            vehicle.Status = "Archived";

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
