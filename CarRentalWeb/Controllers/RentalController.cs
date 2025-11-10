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

        // GET: /Rental
        public IActionResult Index()
        {
            var rentals = _context.Rentals
                .Include(r => r.Customer) // Include Customer details
                .ToList();
            return View(rentals);
        }

        // GET: /Rental/Details/5
        public IActionResult Details(int id)
        {
            var rental = _context.Rentals
                .Include(r => r.Customer)
                .FirstOrDefault(r => r.Id == id);

            if (rental == null)
                return NotFound();

            return View(rental);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Vehicles = _context.Vehicles.ToList();
            return View();
        }


        [HttpPost]
        public IActionResult Create(Rental rental)
        {
            if (ModelState.IsValid)
            {
                _context.Rentals.Add(rental);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Vehicles = _context.Vehicles.ToList();
            return View(rental);
        }

        // GET: /Rental/Delete/5
        public IActionResult Delete(int id)
        {
            var rental = _context.Rentals.Find(id);
            if (rental == null)
                return NotFound();

            _context.Rentals.Remove(rental);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
