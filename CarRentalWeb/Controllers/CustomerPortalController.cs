using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalWeb.Models;

namespace VehicleRentalWeb.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerPortalController : Controller
    {
        private readonly RentalContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerPortalController(RentalContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // -------------------- HOME --------------------
        public IActionResult Home()
        {
            return View();
        }

        // -------------------- MY RENTALS --------------------
        public async Task<IActionResult> MyRentals()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
            {
                TempData["Error"] = "Customer profile not found.";
                return RedirectToAction("Home");
            }

            var rentals = await _context.Rentals
                .Include(r => r.Vehicle)
                .Where(r => r.CustomerId == customer.Id)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();

            return View(rentals);
        }

        // -------------------- BROWSE VEHICLES --------------------
        public async Task<IActionResult> BrowseVehicles()
        {
            var list = await _context.Vehicles
                .Where(v => !v.IsRemoved && v.Status == "Available")
                .ToListAsync();

            return View(list);
        }

        // -------------------- VEHICLE DETAILS --------------------
        public async Task<IActionResult> VehicleDetails(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // -------------------- REQUEST RENTAL --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRental(int vehicleId, DateTime startDate, DateTime endDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
            {
                TempData["Error"] = "Your customer profile was not found.";
                return RedirectToAction("BrowseVehicles");
            }

            var rental = new Rental
            {
                VehicleId = vehicleId,
                CustomerId = customer.Id,
                StartDate = startDate,
                EndDate = endDate,
                Status = RentalStatus.Pending
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Rental request submitted!";
            return RedirectToAction("MyRentals");
        }

    }
}
