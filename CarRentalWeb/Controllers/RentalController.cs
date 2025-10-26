using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalWeb.Models;
using System.Linq;

namespace VehicleRentalWeb.Controllers
{
    public class RentalController : Controller
    {
        private readonly RentalContext _context;

        public RentalController(RentalContext context)
        {
            _context = context;
        }

        // Show all rentals
        public IActionResult Index()
        {
            var rentals = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .ToList();

            return View(rentals);
        }

        // Create rental (GET)
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Vehicles = _context.Vehicles.Where(v => v.IsAvailable).ToList();
            return View();
        }

        // Create rental (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Rental rental)
        {
            if (ModelState.IsValid)
            {
                var vehicle = _context.Vehicles.Find(rental.VehicleId);
                if (vehicle != null)
                {
                    rental.Vehicle = vehicle;
                    rental.CalculateTotalCost();
                    vehicle.IsAvailable = false;

                    _context.Rentals.Add(rental);
                    _context.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            // Reload dropdowns if validation fails
            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Vehicles = _context.Vehicles.Where(v => v.IsAvailable).ToList();
            return View(rental);
        }

        // Rental details
        public IActionResult Details(int id)
        {
            var rental = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefault(r => r.Id == id);

            if (rental == null) return NotFound();
            return View(rental);
        }

        // Delete rental (GET)
        public IActionResult Delete(int id)
        {
            var rental = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefault(r => r.Id == id);

            if (rental == null) return NotFound();
            return View(rental);
        }

        // Delete rental (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var rental = _context.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefault(r => r.Id == id);

            if (rental != null)
            {
                // Mark vehicle available again
                if (rental.Vehicle != null)
                    rental.Vehicle.IsAvailable = true;

                _context.Rentals.Remove(rental);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
