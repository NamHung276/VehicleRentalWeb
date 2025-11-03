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


        // ---------------------- DELETE ----------------------
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _context.Cars.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle != null)
            {
                _context.Cars.Remove(vehicle);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
