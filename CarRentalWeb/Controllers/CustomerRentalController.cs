using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalWeb.Models;

namespace VehicleRentalWeb.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerRentalController : Controller
    {
        private readonly RentalContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerRentalController(RentalContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =====================================================
        // VIEW ALL RENTALS OF THE CURRENT CUSTOMER
        // =====================================================
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null) return NotFound("Customer profile not found.");

            var rentals = await _context.Rentals
                .Include(r => r.Vehicle)
                .Where(r => r.CustomerId == customer.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(rentals);
        }

        // =====================================================
        // CREATE RENTAL (GET)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Vehicles = await _context.Vehicles
                .Where(v => !v.IsRemoved && v.Status == "Available")
                .OrderBy(v => v.Make)
                .ToListAsync();

            return View();
        }

        // =====================================================
        // CREATE RENTAL (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rental rental)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null) return NotFound("Customer profile not found.");

            // Re-populate dropdown if validation fails
            ViewBag.Vehicles = await _context.Vehicles
                .Where(v => !v.IsRemoved && v.Status == "Available")
                .ToListAsync();

            // Validate rental dates
            if (rental.EndDate <= rental.StartDate)
            {
                ModelState.AddModelError("", "End date must be later than start date.");
                return View(rental);
            }

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == rental.VehicleId);

            if (vehicle == null || vehicle.IsRemoved || vehicle.Status != "Available")
            {
                ModelState.AddModelError("", "This vehicle cannot be rented.");
                return View(rental);
            }

            // Calculate rental fee
            int days = (int)(rental.EndDate - rental.StartDate).TotalDays;
            if (days < 1) days = 1;

            rental.RentalFee = vehicle.RatePerDay * days;

            rental.CustomerId = customer.Id;
            rental.Status = RentalStatus.Pending;
            rental.CreatedAt = DateTime.Now;

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Rental request submitted successfully!";
            return RedirectToAction("Home", "CustomerPortal");
        }

        // =====================================================
        // CANCEL RENTAL (ONLY PENDING)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRental(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null) return NotFound("Customer profile not found.");

            var rental = await _context.Rentals
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == customer.Id);

            if (rental == null || rental.Status != RentalStatus.Pending)
            {
                TempData["Error"] = "This rental cannot be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            _context.Rentals.Remove(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Pending rental cancelled successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
