using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VehicleRentalWeb.Models;

namespace VehicleRentalWeb.Controllers
{
    public class RentalController : Controller
    {
        private readonly RentalContext _context;

        public RentalController(RentalContext context)
        {
            _context = context;
        }

        // -------------------------------------------------------
        // LIST ALL RENTALS
        // -------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var rentals = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(rentals);
        }

        // -------------------------------------------------------
        // RENTAL DETAILS
        // -------------------------------------------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null) return NotFound();

            return View(rental);
        }

        // -------------------------------------------------------
        // CREATE RENTAL (GET)
        // -------------------------------------------------------
        public IActionResult Create()
        {
            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Vehicles = _context.Vehicles.Where(v => !v.IsRemoved && v.Status == "Available").ToList();
            return View();
        }

        // -------------------------------------------------------
        // CREATE RENTAL (POST)
        // -------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rental rental)
        {
            if (rental.EndDate < rental.StartDate)
                ModelState.AddModelError("", "End date cannot be before start date.");

            bool overlap = _context.Rentals.Any(r =>
                r.VehicleId == rental.VehicleId &&
                r.Status != RentalStatus.Cancelled &&
                r.Status != RentalStatus.Rejected &&
                r.EndDate >= rental.StartDate &&
                rental.EndDate >= r.StartDate
            );

            if (overlap)
                ModelState.AddModelError("", "Vehicle is already booked for these dates.");

            if (!ModelState.IsValid)
            {
                ViewBag.Customers = _context.Customers.ToList();
                ViewBag.Vehicles = _context.Vehicles.Where(v => !v.IsRemoved && v.Status == "Available").ToList();
                return View(rental);
            }

            rental.CreatedAt = DateTime.Now;
            rental.Status = RentalStatus.Pending;
            rental.PaymentStatus = PaymentStatus.Pending;

            // Calculate cost: 50 per day (modify if needed)
            rental.TotalCost = (decimal)(rental.EndDate - rental.StartDate).TotalDays * 50;

            _context.Add(rental);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // -------------------------------------------------------
        // EDIT RENTAL (GET)
        // -------------------------------------------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            if (!rental.IsActive)
                return BadRequest("Cannot edit a completed, cancelled, or rejected rental.");

            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Vehicles = _context.Vehicles.Where(v => !v.IsRemoved && v.Status == "Available").ToList();

            return View(rental);
        }

        // -------------------------------------------------------
        // EDIT RENTAL (POST)
        // -------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Rental rental)
        {
            var existing = await _context.Rentals.FindAsync(rental.Id);
            if (existing == null) return NotFound();

            if (!existing.IsActive)
                return BadRequest("Cannot edit this rental anymore.");

            bool overlap = _context.Rentals.Any(x =>
                x.Id != rental.Id &&
                x.VehicleId == rental.VehicleId &&
                x.Status != RentalStatus.Cancelled &&
                x.Status != RentalStatus.Rejected &&
                x.EndDate >= rental.StartDate &&
                rental.EndDate >= x.StartDate
            );

            if (overlap)
                ModelState.AddModelError("", "Vehicle is already booked for these dates.");

            if (!ModelState.IsValid)
            {
                ViewBag.Customers = _context.Customers.ToList();
                ViewBag.Vehicles = _context.Vehicles.Where(v => !v.IsRemoved && v.Status == "Available").ToList();
                return View(rental);
            }

            // Apply updates
            existing.CustomerId = rental.CustomerId;
            existing.VehicleId = rental.VehicleId;
            existing.StartDate = rental.StartDate;
            existing.EndDate = rental.EndDate;
            existing.TotalCost = (decimal)(rental.EndDate - rental.StartDate).TotalDays * 50;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // -------------------------------------------------------
        // PAY RENTAL
        // -------------------------------------------------------
        public async Task<IActionResult> Pay(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            if (rental.PaymentStatus == PaymentStatus.Paid)
                return BadRequest("Payment already completed.");

            rental.PaymentStatus = PaymentStatus.Paid;
            rental.Status = RentalStatus.Approved;
            rental.PaymentDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // -------------------------------------------------------
        // REJECT RENTAL
        // -------------------------------------------------------
        public async Task<IActionResult> Reject(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            rental.Status = RentalStatus.Rejected;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // -------------------------------------------------------
        // CANCEL RENTAL
        // -------------------------------------------------------
        public async Task<IActionResult> Cancel(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            if (!rental.IsActive)
                return BadRequest("Rental is already completed or cancelled.");

            rental.Status = RentalStatus.Cancelled;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
