using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalWeb.Models;

namespace VehicleRentalWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly RentalContext _context;

        public AdminDashboardController(RentalContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction("Login", "Account");

            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalCustomers = _context.Customers.Count();
            ViewBag.TotalRentals = _context.Rentals.Count();

            // FIXED: NotMapped property moved to in-memory evaluation
            ViewBag.ActiveRentals = _context.Rentals
                .AsEnumerable()
                .Count(r => r.IsActive);

            ViewBag.TotalVehicles = _context.Vehicles.Count();

            var recentRentals = _context.Rentals
                .Include(r => r.Customer)
                .OrderByDescending(r => r.StartDate)
                .Take(5)
                .ToList();

            return View(recentRentals);
        }
    }
}
