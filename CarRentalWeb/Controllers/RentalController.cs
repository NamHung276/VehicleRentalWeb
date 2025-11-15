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

        // ---------- INDEX (ACTIVE RENTALS) ----------
        public async Task<IActionResult> Index()
        {
            var rentals = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Where(r => !r.IsArchived)
                .ToListAsync();

            return View(rentals);
        }

        // ---------- ARCHIVED ----------
        public async Task<IActionResult> Archived()
        {
            var archived = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Where(r => r.IsArchived)
                .ToListAsync();

            return View(archived);
        }

        // ---------- DETAILS ----------
        public async Task<IActionResult> Details(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null) return NotFound();

            return View(rental);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _context.Customers
                .Select(c => new {
                    c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            ViewBag.Vehicles = await _context.Vehicles
                .Where(v => !v.IsRemoved && v.Status == "Available")
                .Select(v => new {
                    v.Id,
                    Display = v.Make + " " + v.Model + " (" + v.Year + ")"
                })
                .ToListAsync();

            return View();
        }

        // ---------- CREATE (POST) ----------
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            ViewBag.Customers = await _context.Customers
                .Select(c => new {
                    c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            ViewBag.Vehicles = await _context.Vehicles
                .Where(v => !v.IsRemoved)
                .Select(v => new {
                    v.Id,
                    Display = v.Make + " " + v.Model + " (" + v.Year + ")"
                })
                .ToListAsync();

            return View(rental);
        }

        // ---------- EDIT (POST) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Rental rental)
        {
            if (id != rental.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Vehicles = await _context.Vehicles.Where(v => !v.IsRemoved).ToListAsync();
                ViewBag.Customers = await _context.Customers.ToListAsync();
                return View(rental);
            }

            _context.Update(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Rental updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ---------- DELETE (GET CONFIRM PAGE) ----------
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

        // ---------- ARCHIVE (SOFT DELETE) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            rental.IsArchived = true;
            _context.Update(rental);

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Rental ID {id} archived successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- RESTORE ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            rental.IsArchived = false;
            _context.Update(rental);

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Rental ID {id} restored!";
            return RedirectToAction(nameof(Archived));
        }

        // ---------- DELETE PERMANENT ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermanent(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            _context.Rentals.Remove(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Rental ID {id} permanently deleted!";
            return RedirectToAction(nameof(Archived));
        }
    }
}
