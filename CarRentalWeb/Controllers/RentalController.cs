using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VehicleRentalWeb.Models;

namespace VehicleRentalWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RentalController : Controller
    {
        private readonly RentalContext _context;

        public RentalController(RentalContext context)
        {
            _context = context;
        }

        // ===========================
        // INDEX: LIST ALL RENTALS
        // ===========================
        public async Task<IActionResult> Index()
        {
            var rentals = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(rentals);
        }

        // ===========================
        // DETAILS
        // ===========================
        public async Task<IActionResult> Details(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null) return NotFound();

            return View(rental);
        }

        // ===========================
        // APPROVE RENTAL
        // ===========================
        public async Task<IActionResult> Approve(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null) return NotFound();

            rental.Status = RentalStatus.Approved;

            if (rental.Vehicle != null)
            {
                rental.Vehicle.Status = "Rented";
                _context.Update(rental.Vehicle);
            }

            _context.Update(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Rental approved successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // REJECT RENTAL
        // ===========================
        public async Task<IActionResult> Reject(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            rental.Status = RentalStatus.Rejected;
            _context.Update(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Rental rejected.";
            return RedirectToAction(nameof(Index));
        }
    }
}
