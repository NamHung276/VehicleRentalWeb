using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VehicleRentalWeb.Models;

namespace VehicleRentalWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RentalController : Controller
    {
        private readonly RentalContext _context;

        public RentalController(RentalContext context)
        {
            _context = context;
        }

        // ===========================
        // ADMIN: ALL RENTALS
        // ===========================
        public async Task<IActionResult> Index()
        {
            var rentals = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(rentals);
        }


        // ===========================
        // DETAILS
        // ===========================
        public async Task<IActionResult> Details(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null) return NotFound();

            return View(rental);
        }


        // ===========================
        // CREATE (GET)
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _context.Customers
                .Where(c => !c.IsArchived)
                .ToListAsync();

            ViewBag.Vehicles = await _context.Vehicles
                .Where(v => !v.IsRemoved && v.Status == "Available")
                .ToListAsync();

            return View();
        }

        // ===========================
        // CREATE (POST)
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rental rental)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Customers = await _context.Customers.ToListAsync();
                ViewBag.Vehicles = await _context.Vehicles.ToListAsync();
                return View(rental);
            }

            _context.Rentals.Add(rental);

            // Update vehicle status
            var vehicle = await _context.Vehicles.FindAsync(rental.VehicleId);
            if (vehicle != null)
            {
                vehicle.Status = "Rented";
                _context.Update(vehicle);
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Rental successfully created!";

            return RedirectToAction(nameof(Index));
        }


        // ===========================
        // EDIT (GET)
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            ViewBag.Customers = await _context.Customers
                .Where(c => !c.IsArchived)
                .ToListAsync();

            ViewBag.Vehicles = await _context.Vehicles
                .Where(v => !v.IsRemoved)
                .ToListAsync();

            return View(rental);
        }


        // ===========================
        // EDIT (POST)
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Rental rental)
        {
            if (id != rental.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Customers = await _context.Customers.ToListAsync();
                ViewBag.Vehicles = await _context.Vehicles.ToListAsync();
                return View(rental);
            }

            _context.Update(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Rental updated successfully!";
            return RedirectToAction(nameof(Index));
        }


        // ===========================
        // DELETE (SOFT — ARCHIVED)
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null) return NotFound();

            return View(rental);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            // Soft delete = mark as rejected
            rental.Status = RentalStatus.Rejected;
            _context.Update(rental);

            await _context.SaveChangesAsync();
            TempData["Message"] = "Rental archived (marked Rejected).";

            return RedirectToAction(nameof(Index));
        }
    }
}
