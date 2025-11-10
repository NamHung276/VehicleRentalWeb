using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalWeb.Models;

namespace VehicleRentalWeb.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly RentalContext _context;

        public AdminDashboardController(RentalContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Check if user is logged in and is an Admin
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            // Dashboard statistics
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalCustomers = _context.Customers.Count();
            ViewBag.TotalRentals = _context.Rentals.Count();
            ViewBag.ActiveRentals = _context.Rentals.Count(r => r.IsActive);
            ViewBag.TotalVehicles = _context.Vehicles.Count();

            // Optional: list of recent rentals
            var recentRentals = _context.Rentals
                .Include(r => r.Customer)
                .OrderByDescending(r => r.StartDate)
                .Take(5)
                .ToList();

            return View(recentRentals);
        }
    }
}
