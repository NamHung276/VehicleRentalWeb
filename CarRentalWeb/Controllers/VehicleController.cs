using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalWeb.Models;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace VehicleRentalWeb.Controllers
{
    public class VehicleController : Controller
    {
        private readonly RentalContext _context;
        private readonly string _imageFolder = "wwwroot/images";

        public VehicleController(RentalContext context)
        {
            _context = context;
        }

        // ---------- INDEX ----------
        public async Task<IActionResult> Index()
        {
            var cars = await _context.Cars.Where(c => !c.IsRemoved).ToListAsync();
            var bikes = await _context.Bikes.Where(b => !b.IsRemoved).ToListAsync();
            var trucks = await _context.Trucks.Where(t => !t.IsRemoved).ToListAsync();

            var vehicles = cars.Cast<Vehicle>()
                .Concat(bikes)
                .Concat(trucks)
                .ToList();

            return View(vehicles);
        }

        // ---------- DETAILS ----------
        public async Task<IActionResult> Details(int id)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);
            if (car != null) return View("DetailsCar", car);

            var bike = await _context.Bikes.FirstOrDefaultAsync(b => b.Id == id);
            if (bike != null) return View("DetailsBike", bike);

            var truck = await _context.Trucks.FirstOrDefaultAsync(t => t.Id == id);
            if (truck != null) return View("DetailsTruck", truck);

            return NotFound();
        }

        // ---------- CREATE (GET) ----------
        public IActionResult Create()
        {
            return View();
        }

        // ---------- CREATE (POST) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string vehicleType,
            string Make, string Model, int Year, string Color, string Supplier,
            decimal RatePerDay, string Status,
            IFormFile? ImageFile,
            int? Seats, string? Luggage, string? Transmission, bool? HasAC, bool? HasCarPlay,
            string? FuelType, string? MileagePolicy,
            int? EngineCC, bool? IsElectric,
            double? CapacityInTons)
        {
            if (string.IsNullOrEmpty(vehicleType))
                return BadRequest("Vehicle type required");

            Vehicle newVehicle;

            switch (vehicleType)
            {
                case "Car":
                    newVehicle = new Car
                    {
                        Make = Make,
                        Model = Model,
                        Year = Year,
                        Color = Color,
                        Supplier = Supplier,
                        RatePerDay = RatePerDay,
                        Status = Status,
                        Seats = Seats ?? 4,
                        Luggage = Luggage ?? "1 Large, 1 Small",
                        Transmission = Transmission ?? "Automatic",
                        HasAC = HasAC ?? true,
                        HasCarPlay = HasCarPlay ?? true,
                        FuelType = FuelType ?? "Fuel",
                        MileagePolicy = MileagePolicy ?? "Unlimited"
                    };
                    _context.Cars.Add((Car)newVehicle);
                    break;

                case "Bike":
                    newVehicle = new Bike
                    {
                        Make = Make,
                        Model = Model,
                        Year = Year,
                        Color = Color,
                        Supplier = Supplier,
                        RatePerDay = RatePerDay,
                        Status = Status,
                        EngineCC = EngineCC ?? 150,
                        Transmission = Transmission ?? "Manual",
                        IsElectric = IsElectric ?? false
                    };
                    _context.Bikes.Add((Bike)newVehicle);
                    break;

                case "Truck":
                    newVehicle = new Truck
                    {
                        Make = Make,
                        Model = Model,
                        Year = Year,
                        Color = Color,
                        Supplier = Supplier,
                        RatePerDay = RatePerDay,
                        Status = Status,
                        Transmission = Transmission ?? "Manual",
                        HasAC = HasAC ?? true,
                        CapacityInTons = CapacityInTons ?? 2.0
                    };
                    _context.Trucks.Add((Truck)newVehicle);
                    break;

                default:
                    return BadRequest("Invalid vehicle type");
            }

            // ---- Image Upload ----
            if (ImageFile != null && ImageFile.Length > 0)
            {
                Directory.CreateDirectory(_imageFolder);
                string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                string path = Path.Combine(_imageFolder, fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await ImageFile.CopyToAsync(stream);
                newVehicle.ImagePath = "/images/" + fileName;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ---------- EDIT (GET) ----------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            if (vehicle is Car) return View("EditCar", (Car)vehicle);
            if (vehicle is Bike) return View("EditBike", (Bike)vehicle);
            if (vehicle is Truck) return View("EditTruck", (Truck)vehicle);

            return NotFound();
        }

        // ---------- EDIT CAR ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCar(int id, Car car, IFormFile? ImageFile)
        {
            if (id != car.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        Directory.CreateDirectory(_imageFolder);
                        string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                        string path = Path.Combine(_imageFolder, fileName);
                        using var stream = new FileStream(path, FileMode.Create);
                        await ImageFile.CopyToAsync(stream);
                        car.ImagePath = "/images/" + fileName;
                    }

                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Vehicles.Any(e => e.Id == car.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }

        // ---------- EDIT BIKE ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBike(int id, Bike bike, IFormFile? ImageFile)
        {
            if (id != bike.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        Directory.CreateDirectory(_imageFolder);
                        string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                        string path = Path.Combine(_imageFolder, fileName);
                        using var stream = new FileStream(path, FileMode.Create);
                        await ImageFile.CopyToAsync(stream);
                        bike.ImagePath = "/images/" + fileName;
                    }

                    _context.Update(bike);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Vehicles.Any(e => e.Id == bike.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bike);
        }

        // ---------- EDIT TRUCK ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTruck(int id, Truck truck, IFormFile? ImageFile)
        {
            if (id != truck.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        Directory.CreateDirectory(_imageFolder);
                        string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                        string path = Path.Combine(_imageFolder, fileName);
                        using var stream = new FileStream(path, FileMode.Create);
                        await ImageFile.CopyToAsync(stream);
                        truck.ImagePath = "/images/" + fileName;
                    }

                    _context.Update(truck);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Vehicles.Any(e => e.Id == truck.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(truck);
        }

        // ---------- DELETE (ARCHIVE) ----------
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            vehicle.IsRemoved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ---------- ARCHIVED LIST ----------
        public async Task<IActionResult> Archived()
        {
            var cars = await _context.Cars.Where(c => c.IsRemoved).ToListAsync();
            var bikes = await _context.Bikes.Where(b => b.IsRemoved).ToListAsync();
            var trucks = await _context.Trucks.Where(t => t.IsRemoved).ToListAsync();

            var archivedVehicles = cars.Cast<Vehicle>()
                .Concat(bikes)
                .Concat(trucks)
                .ToList();

            return View(archivedVehicles);
        }

        // ---------- RESTORE ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            vehicle.IsRemoved = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Archived));
        }

        // ---------- PERMANENT DELETE ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermanent(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Archived));
        }

    }
}
