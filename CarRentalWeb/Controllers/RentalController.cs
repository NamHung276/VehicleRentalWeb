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

        public IActionResult Index()
        {
            var rentals = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .ToList();

            return View(rentals);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Vehicles = _context.Vehicles.ToList();
            ViewBag.Customers = _context.Customers.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Rental rental)
        {
            if (ModelState.IsValid)
            {
                _context.Rentals.Add(rental);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Vehicles = _context.Vehicles.ToList();
            ViewBag.Customers = _context.Customers.ToList();
            return View(rental);
        }
    }
}
