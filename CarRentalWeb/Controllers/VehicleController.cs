using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalWeb.Models;
using System.Threading.Tasks;
using System.Linq;

namespace VehicleRentalWeb.Controllers
{
    public class VehicleController : Controller
    {
        private readonly RentalContext _context;

        public VehicleController(RentalContext context)
        {
            _context = context;
        }

        // ---------------------- INDEX ----------------------
        public async Task<IActionResult> Index()
        {
            // Load all Cars (VehicleType = "Car")
            var vehicles = await _context.Vehicles
                .OfType<Car>()
                .Where(v => !v.IsRemoved)
                .ToListAsync();

            return View(vehicles);
        }

        // ---------------------- DETAILS ----------------------
        public async Task<IActionResult> Details(int id)
        {
            var vehicle = await _context.Cars.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null) return NotFound();
            return View(vehicle);
        }

        // ---------------------- CREATE ----------------------
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Car vehicle, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload if provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    vehicle.ImagePath = "/images/" + uniqueFileName;
                }
                else
                {
                    // Default if no image uploaded
                    vehicle.ImagePath = "/images/default_car.jpg";
                }

                // Ensure EF recognizes it as Car (VehicleType = "Car")
                _context.Cars.Add(vehicle);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(vehicle);
        }


        // ---------------------- DELETE (Soft Delete) ----------------------
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _context.Cars.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
                return NotFound();

            return View(vehicle); // Show confirmation page
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Cars.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
                return NotFound();

            // Mark as removed (soft delete)
            vehicle.IsRemoved = true;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Vehicle removed successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------- ARCHIVED VEHICLES ----------------------
        public async Task<IActionResult> Archived()
        {
            // Load vehicles that were soft-deleted
            var archivedVehicles = await _context.Vehicles
                .OfType<Car>()
                .Where(v => v.IsRemoved)
                .ToListAsync();

            return View(archivedVehicles);
        }

        // ---------------------- RESTORE VEHICLE ----------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var vehicle = await _context.Cars.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
                return NotFound();

            vehicle.IsRemoved = false;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{vehicle.Make} {vehicle.Model} restored successfully.";
            return RedirectToAction(nameof(Archived));
        }

        // ---------------------- EDIT (GET) ----------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vehicle = await _context.Cars.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null) return NotFound();
            return View(vehicle);
        }

        // ---------------------- EDIT (POST) ----------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Car formModel, IFormFile? ImageFile)
        {
            if (id != formModel.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(formModel);
            }

            var vehicle = await _context.Cars.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null) return NotFound();

            // Update details
            vehicle.Make = formModel.Make;
            vehicle.Model = formModel.Model;
            vehicle.Year = formModel.Year;
            vehicle.Color = formModel.Color;
            vehicle.Supplier = formModel.Supplier;
            vehicle.RatePerDay = formModel.RatePerDay;
            vehicle.IsAvailable = formModel.IsAvailable;
            vehicle.Seats = formModel.Seats;
            vehicle.FuelType = formModel.FuelType;

            // Handle optional image upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder);

                // Delete old file if not default
                if (!string.IsNullOrEmpty(vehicle.ImagePath) && !vehicle.ImagePath.Contains("default_car.jpg"))
                {
                    string oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", vehicle.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                vehicle.ImagePath = "/images/" + uniqueFileName;
            }

            _context.Update(vehicle);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Vehicle updated successfully!";
            return RedirectToAction(nameof(Index));
        }

    }
}
