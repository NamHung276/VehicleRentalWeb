using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using VehicleRentalWeb.Models;

namespace VehicleRentalWeb.Controllers
{
    public class VehicleController : Controller
    {
        // Temporary static list (will later move to database)
        // Replace 'Vehicle' with a concrete class, e.g., 'Car'
        private static List<Car> vehicles = new List<Car>
{
    new Car {
        Id = 1,
        Make = "Toyota",
        Model = "Camry",
        Year = 2020,
        Color = "White",
        Supplier = "Alice",
        RatePerDay = 50m,
        ImagePath = "/images/toyota_camry.jpg"
    },
    new Car {
        Id = 2,
        Make = "Honda",
        Model = "Civic",
        Year = 2021,
        Color = "Blue",
        Supplier = "Bob",
        RatePerDay = 45m,
        ImagePath = "/images/honda_civic.jpg"
    }
};


        public IActionResult Index()
        {
            return View(vehicles);
        }

        public IActionResult Details(int id)
        {
            var vehicle = vehicles.FirstOrDefault(v => v.Id == id);
            if (vehicle == null) return NotFound();
            return View(vehicle);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Car vehicle)
        {
            vehicle.Id = vehicles.Max(v => v.Id) + 1;
            vehicles.Add(vehicle);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var vehicle = vehicles.FirstOrDefault(v => v.Id == id);
            if (vehicle != null)
                vehicles.Remove(vehicle);

            return RedirectToAction("Index");
        }
    }
}
