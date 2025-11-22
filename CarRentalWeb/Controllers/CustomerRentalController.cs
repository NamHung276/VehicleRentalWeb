using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
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

        // View my rentals
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null) return NotFound();

            var rentals = await _context.Rentals
                .Include(r => r.Vehicle)
                .Where(r => r.CustomerId == customer.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(rentals);
        }

        // Create rental (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Vehicles = await _context.Vehicles
                .Where(v => !v.IsRemoved && v.Status == "Available")
                .ToListAsync();
            return View();
        }

        // Create rental (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rental rental)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null) return NotFound();

            ViewBag.Vehicles = await _context.Vehicles
                .Where(v => !v.IsRemoved && v.Status == "Available")
                .ToListAsync();

            if (rental.EndDate <= rental.StartDate)
            {
                ModelState.AddModelError("", "End date must be later than start date.");
                return View(rental);
            }

            var vehicle = await _context.Vehicles.FindAsync(rental.VehicleId);
            if (vehicle == null || vehicle.IsRemoved || vehicle.Status != "Available")
            {
                ModelState.AddModelError("", "Vehicle not available.");
                return View(rental);
            }

            int days = (int)(rental.EndDate - rental.StartDate).TotalDays;
            if (days < 1) days = 1;
            rental.TotalCost = vehicle.RatePerDay * days; // <-- corrected

            rental.CustomerId = customer.Id;
            rental.Status = RentalStatus.Pending;
            rental.PaymentStatus = PaymentStatus.Pending;
            rental.CreatedAt = DateTime.Now;

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Rental request submitted! Please pay to confirm.";
            return RedirectToAction(nameof(Index));
        }

        // Cancel rental
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRental(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null) return NotFound();

            var rental = await _context.Rentals.Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == customer.Id);

            if (rental == null)
            {
                TempData["Error"] = "Rental not found.";
                return RedirectToAction(nameof(Index));
            }

            if (rental.PaymentStatus == PaymentStatus.Paid)
            {
                TempData["Error"] = "Cannot cancel a paid rental. Contact admin.";
                return RedirectToAction(nameof(Index));
            }

            if (DateTime.Now >= rental.StartDate)
            {
                TempData["Error"] = "Cannot cancel a rental that has already started.";
                return RedirectToAction(nameof(Index));
            }

            rental.Status = RentalStatus.Cancelled;
            _context.Rentals.Update(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Rental cancelled.";
            return RedirectToAction(nameof(Index));
        }
    }
}
