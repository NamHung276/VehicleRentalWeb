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
    public class CustomerPortalController : Controller
    {
        private readonly RentalContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerPortalController(RentalContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Home() => View();

        // Browse available vehicles
        public async Task<IActionResult> BrowseVehicles()
        {
            var vehicles = await _context.Vehicles
                .Where(v => !v.IsRemoved && v.Status == "Available")
                .ToListAsync();
            return View(vehicles);
        }

        // Vehicle details
        public async Task<IActionResult> VehicleDetails(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();
            return View(vehicle);
        }

        // Request rental
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRental(int vehicleId, DateTime startDate, DateTime endDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null)
            {
                TempData["Error"] = "Customer profile not found.";
                return RedirectToAction(nameof(BrowseVehicles));
            }

            if (endDate <= startDate)
            {
                TempData["Error"] = "End date must be later than start date.";
                return RedirectToAction(nameof(VehicleDetails), new { id = vehicleId });
            }

            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null || vehicle.IsRemoved || vehicle.Status != "Available")
            {
                TempData["Error"] = "Vehicle not available.";
                return RedirectToAction(nameof(BrowseVehicles));
            }

            int days = (int)(endDate - startDate).TotalDays;
            if (days < 1) days = 1;
            var totalCost = vehicle.RatePerDay * days;

            var rental = new Rental
            {
                VehicleId = vehicleId,
                CustomerId = customer.Id,
                StartDate = startDate,
                EndDate = endDate,
                TotalCost = totalCost,
                Status = RentalStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Rental requested — total {totalCost:C}. Please pay to confirm.";
            return RedirectToAction(nameof(MyRentals));
        }

        // My rentals
        public async Task<IActionResult> MyRentals()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null)
            {
                TempData["Error"] = "Customer profile not found.";
                return RedirectToAction(nameof(Home));
            }

            var rentals = await _context.Rentals
                .Include(r => r.Vehicle)
                .Where(r => r.CustomerId == customer.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(rentals);
        }

        // Pay for rental
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayRental(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null) return NotFound();

            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == customer.Id);

            if (rental == null) return NotFound();

            if (rental.PaymentStatus == PaymentStatus.Paid)
            {
                TempData["Error"] = "Rental already paid.";
                return RedirectToAction(nameof(MyRentals));
            }

            rental.PaymentStatus = PaymentStatus.Paid;
            rental.PaymentDate = DateTime.Now;
            rental.TransactionId = $"SIM-{Guid.NewGuid().ToString().Split('-')[0].ToUpper()}";

            _context.Rentals.Update(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Payment successful!";
            return RedirectToAction(nameof(MyRentals));
        }
    }
}
