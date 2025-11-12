using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
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

        // ---------------------- INDEX ----------------------
        public async Task<IActionResult> Index()
        {
            var rentals = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .ToListAsync();
            return View(rentals);
        }

        // ---------------------- DETAILS ----------------------
        public async Task<IActionResult> Details(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
                return NotFound();

            return View(rental);
        }

        // ---------------------- CREATE (GET) ----------------------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Vehicles = await _context.Vehicles
                .Where(v => !v.IsRemoved && v.Status == "Available")
                .ToListAsync();

            ViewBag.Customers = await _context.Customers.ToListAsync();
            return View();
        }

        // ---------------------- CREATE (POST) ----------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rental rental)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Vehicles = await _context.Vehicles
                    .Where(v => !v.IsRemoved && v.Status == "Available")
                    .ToListAsync();
                ViewBag.Customers = await _context.Customers.ToListAsync();
                return View(rental);
            }

            // Add rental to DB
            _context.Rentals.Add(rental);

            // Update vehicle status
            var vehicle = await _context.Vehicles.FindAsync(rental.VehicleId);
            if (vehicle != null)
            {
                vehicle.Status = "Rented";
                _context.Vehicles.Update(vehicle);
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Rental successfully created!";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------- EDIT ----------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null)
                return NotFound();

            ViewBag.Vehicles = await _context.Vehicles
                .Where(v => !v.IsRemoved)
                .ToListAsync();
            ViewBag.Customers = await _context.Customers.ToListAsync();
            return View(rental);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Rental rental)
        {
            if (id != rental.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Vehicles = await _context.Vehicles
                    .Where(v => !v.IsRemoved)
                    .ToListAsync();
                ViewBag.Customers = await _context.Customers.ToListAsync();
                return View(rental);
            }

            _context.Update(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Rental updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------- DELETE ----------------------
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
                return NotFound();

            return View(rental);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null)
                return NotFound();

            // Restore vehicle status when rental deleted
            var vehicle = await _context.Vehicles.FindAsync(rental.VehicleId);
            if (vehicle != null)
            {
                vehicle.Status = "Available";
                _context.Vehicles.Update(vehicle);
            }

            _context.Rentals.Remove(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Rental deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
