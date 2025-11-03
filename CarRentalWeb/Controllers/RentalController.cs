using Microsoft.AspNetCore.Mvc;
using VehicleRentalWeb.Models;
using System.Collections.Generic;
using System.Linq;

namespace VehicleRentalWeb.Controllers
{
    public class RentalController : Controller
    {
        private static List<Rental> rentals = new List<Rental>
        {
            new Rental { Id = 1, CustomerName = "Alice", VehicleName = "Toyota Camry", StartDate = DateTime.Today.AddDays(-3), EndDate = DateTime.Today, TotalCost = 150, IsActive = false },
            new Rental { Id = 2, CustomerName = "Bob", VehicleName = "Honda Civic", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), TotalCost = 90, IsActive = true }
        };

        public IActionResult Index() => View(rentals);

        public IActionResult Details(int id)
        {
            var rental = rentals.FirstOrDefault(r => r.Id == id);
            if (rental == null) return NotFound();
            return View(rental);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(Rental rental)
        {
            rental.Id = rentals.Max(r => r.Id) + 1;
            rentals.Add(rental);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var rental = rentals.FirstOrDefault(r => r.Id == id);
            if (rental != null)
                rentals.Remove(rental);
            return RedirectToAction("Index");
        }
    }
}
