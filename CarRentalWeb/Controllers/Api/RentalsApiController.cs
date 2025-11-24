using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalWeb.Models;

namespace VehicleRentalWeb.Api
{
    [Route("api/rentals")]
    [ApiController]
    public class RentalsApiController : ControllerBase
    {
        private readonly RentalContext _context;

        public RentalsApiController(RentalContext context)
        {
            _context = context;
        }

        // GET: api/rentals
        [HttpGet]
        public async Task<IActionResult> GetAllRentals()
        {
            var rentals = await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.Customer)
                .ToListAsync();

            return Ok(rentals);
        }

        // GET: api/rentals/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRental(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental is null) return NotFound();

            return Ok(rental);
        }

        // POST: api/rentals
        [HttpPost]
        public async Task<IActionResult> CreateRental([FromBody] Rental rental)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            rental.Status = RentalStatus.Pending;
            rental.PaymentStatus = PaymentStatus.Pending;
            rental.CreatedAt = DateTime.Now;

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRental), new { id = rental.Id }, rental);
        }

        // PUT: api/rentals/{id}  (update status/payment)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRental(int id, [FromBody] Rental updatedRental)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental is null) return NotFound();

            rental.Status = updatedRental.Status;
            rental.PaymentStatus = updatedRental.PaymentStatus;
            rental.TotalCost = updatedRental.TotalCost;
            rental.StartDate = updatedRental.StartDate;
            rental.EndDate = updatedRental.EndDate;

            _context.Rentals.Update(rental);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/rentals/{id}  (cancel rental)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelRental(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental is null) return NotFound();

            if (rental.PaymentStatus == PaymentStatus.Paid)
                return BadRequest("Cannot cancel a paid rental");

            rental.Status = RentalStatus.Cancelled;
            _context.Rentals.Update(rental);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
