using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VehicleRentalWeb.Models;
using VehicleRentalWeb.Models.ViewModels;

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

        // ────────────────
        // HOME
        // ────────────────
        public IActionResult Home() => View();

        // ────────────────
        // BROWSE VEHICLES
        // ────────────────
        public async Task<IActionResult> BrowseVehicles(
            string search, string type, string fuel, string trans, string status, string sort)
        {
            // Base query
            var vehiclesQuery = _context.Vehicles
                .Where(v => !v.IsRemoved && v.Status == "Available");

            // --------------------
            // Filters that can run in DB
            // --------------------
            if (!string.IsNullOrEmpty(status))
            {
                vehiclesQuery = vehiclesQuery.Where(v => v.Status == status);
            }

            if (!string.IsNullOrEmpty(type))
            {
                type = type.ToLower();
                if (type == "car")
                    vehiclesQuery = vehiclesQuery.OfType<Car>();
                else if (type == "bike")
                    vehiclesQuery = vehiclesQuery.OfType<Bike>();
                else if (type == "truck")
                    vehiclesQuery = vehiclesQuery.OfType<Truck>();
            }

            // Execute base query
            var vehiclesList = await vehiclesQuery.ToListAsync();

            // --------------------
            // Filters that require subclass properties (in-memory)
            // --------------------
            if (!string.IsNullOrEmpty(trans))
            {
                vehiclesList = vehiclesList.Where(v =>
                    (v is Car car && car.Transmission == trans) ||
                    (v is Bike bike && bike.Transmission == trans) ||
                    (v is Truck truck && truck.Transmission == trans)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(fuel))
            {
                vehiclesList = vehiclesList.Where(v =>
                    v is Car car && car.FuelType == fuel
                ).ToList();
            }

            // --------------------
            // Search (in-memory)
            // --------------------
            if (!string.IsNullOrEmpty(search))
            {
                string lowered = search.ToLower();
                vehiclesList = vehiclesList
                    .Where(v => (!string.IsNullOrEmpty(v.Make) && v.Make.ToLower().Contains(lowered))
                             || (!string.IsNullOrEmpty(v.Model) && v.Model.ToLower().Contains(lowered)))
                    .ToList();
            }

            // --------------------
            // Sorting (in-memory)
            // --------------------
            vehiclesList = sort switch
            {
                "price_asc" => vehiclesList.OrderBy(v => v.RatePerDay).ToList(),
                "price_desc" => vehiclesList.OrderByDescending(v => v.RatePerDay).ToList(),
                "year_new" => vehiclesList.OrderByDescending(v => v.Year).ToList(),
                "year_old" => vehiclesList.OrderBy(v => v.Year).ToList(),
                "make_az" => vehiclesList.OrderBy(v => v.Make).ToList(),
                "make_za" => vehiclesList.OrderByDescending(v => v.Make).ToList(),
                _ => vehiclesList
            };

            return View(vehiclesList);
        }

        // ────────────────
        // VEHICLE DETAILS
        // ────────────────
        public async Task<IActionResult> VehicleDetails(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // ────────────────
        // REQUEST RENTAL
        // ────────────────
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

            TempData["Message"] = $"Rental requested — total {totalCost:C}.";
            return RedirectToAction(nameof(MyRentals));
        }

        // ────────────────
        // MY RENTALS
        // ────────────────
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

        // ────────────────
        // PAY RENTAL
        // ────────────────
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

        // ────────────────
        // PROFILE
        // ────────────────
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);

            var model = new CustomerProfileViewModel
            {
                DisplayName = user.DisplayName ?? "Unknown",
                Email = user.Email ?? "Unknown",
                PhoneNumber = user.PhoneNumber ?? "N/A",
                Address = customer?.Address ?? "No address provided"
            };

            return View(model);
        }

        // ────────────────
        // EDIT PROFILE (GET)
        // ────────────────
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);

            var model = new EditProfileViewModel
            {
                DisplayName = user.DisplayName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Address = customer?.Address ?? ""
            };

            return View(model);
        }

        // ────────────────
        // EDIT PROFILE (POST)
        // ────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Update Identity user
            user.DisplayName = model.DisplayName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            // Update Customer table
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer != null)
            {
                customer.Name = model.DisplayName;
                customer.Email = model.Email;
                customer.Phone = model.PhoneNumber;
                customer.Address = model.Address;

                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
            }

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }
    }
}
