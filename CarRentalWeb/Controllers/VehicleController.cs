using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalWeb.Models;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

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
        public async Task<IActionResult> Index(string search, string type, string fuelType, string status, string category)
        {
            IQueryable<Vehicle> vehicles = _context.Vehicles.Where(v => !v.IsRemoved);

            if (!string.IsNullOrEmpty(search))
                vehicles = vehicles.Where(v => v.Make.Contains(search) || v.Model.Contains(search));

            if (!string.IsNullOrEmpty(type))
            {
                vehicles = type switch
                {
                    "Car" => _context.Cars.Where(v => !v.IsRemoved),
                    "Bike" => _context.Bikes.Where(v => !v.IsRemoved),
                    "Truck" => _context.Trucks.Where(v => !v.IsRemoved),
                    _ => vehicles
                };
            }

            if (!string.IsNullOrEmpty(fuelType))
                vehicles = _context.Cars.Where(c => !c.IsRemoved && c.FuelType == fuelType);

            if (!string.IsNullOrEmpty(status))
                vehicles = vehicles.Where(v => v.Status == status);

            if (!string.IsNullOrEmpty(category))
                vehicles = vehicles.Where(v => v.Category == category);

            ViewBag.VehicleTypes = new List<string> { "Car", "Bike", "Truck" };
            ViewBag.CarTypes = new List<string> { "SUV", "Hatchback", "Sedan", "Coupe" };
            ViewBag.BikeTypes = new List<string> { "Scooter", "Sport", "Touring", "Cruiser" };
            ViewBag.TruckTypes = new List<string> { "Pickup", "Cargo", "Lorry", "Trailer" };
            ViewBag.FuelTypes = new List<string> { "Fuel", "Diesel", "Electric", "Hybrid" };
            ViewBag.StatusOptions = new List<string> { "Available", "Rented", "In Service" };

            return View(await vehicles.ToListAsync());
        }

        // ---------------------- DETAILS ----------------------
        public async Task<IActionResult> Details(int id)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null) return NotFound();
            return View(vehicle);
        }

        // ---------------------- CREATE ----------------------
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.VehicleTypes = new List<string> { "Car", "Bike", "Truck" };
            ViewBag.FuelTypes = new List<string> { "Fuel", "Diesel", "Electric", "Hybrid" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string vehicleType, Vehicle baseVehicle, IFormFile? ImageFile,
                                               int? Seats, string? FuelType, string? Category,
                                               int? EngineCC, bool? IsElectric, double? CapacityInTons)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.VehicleTypes = new List<string> { "Car", "Bike", "Truck" };
                ViewBag.FuelTypes = new List<string> { "Fuel", "Diesel", "Electric", "Hybrid" };
                return View(baseVehicle);
            }

            baseVehicle.ImagePath = await SaveImageAsync(ImageFile, "default_car.jpg");

            switch (vehicleType)
            {
                case "Car":
                    _context.Cars.Add(new Car
                    {
                        Make = baseVehicle.Make,
                        Model = baseVehicle.Model,
                        Year = baseVehicle.Year,
                        Color = baseVehicle.Color,
                        Supplier = baseVehicle.Supplier,
                        RatePerDay = baseVehicle.RatePerDay,
                        ImagePath = baseVehicle.ImagePath,
                        Category = Category ?? "Sedan",
                        Status = "Available",
                        Seats = Seats ?? 4,
                        FuelType = FuelType ?? "Fuel"
                    });
                    break;

                case "Bike":
                    _context.Bikes.Add(new Bike
                    {
                        Make = baseVehicle.Make,
                        Model = baseVehicle.Model,
                        Year = baseVehicle.Year,
                        Color = baseVehicle.Color,
                        Supplier = baseVehicle.Supplier,
                        RatePerDay = baseVehicle.RatePerDay,
                        ImagePath = baseVehicle.ImagePath,
                        Category = Category ?? "Standard",
                        EngineCC = EngineCC ?? 150,
                        IsElectric = IsElectric ?? false,
                        Status = "Available"
                    });
                    break;

                case "Truck":
                    _context.Trucks.Add(new Truck
                    {
                        Make = baseVehicle.Make,
                        Model = baseVehicle.Model,
                        Year = baseVehicle.Year,
                        Color = baseVehicle.Color,
                        Supplier = baseVehicle.Supplier,
                        RatePerDay = baseVehicle.RatePerDay,
                        ImagePath = baseVehicle.ImagePath,
                        Category = Category ?? "Pickup",
                        CapacityInTons = CapacityInTons ?? 2,
                        Status = "Available"
                    });
                    break;

                default:
                    ModelState.AddModelError("", "Please select a valid vehicle type.");
                    return View(baseVehicle);
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = $"{baseVehicle.Make} {baseVehicle.Model} added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------- EDIT ----------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null) return NotFound();

            // Return a strongly typed view based on type
            if (vehicle is Car car)
                return View("Edit", car);
            if (vehicle is Bike bike)
                return View("EditBike", bike);
            if (vehicle is Truck truck)
                return View("EditTruck", truck);

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCar(int id, Car updatedCar, IFormFile? ImageFile)
        {
            if (id != updatedCar.Id) return NotFound();

            if (!ModelState.IsValid)
                return View("Edit", updatedCar);

            var existingCar = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);
            if (existingCar == null) return NotFound();

            // Update basic info
            existingCar.Make = updatedCar.Make;
            existingCar.Model = updatedCar.Model;
            existingCar.Year = updatedCar.Year;
            existingCar.Color = updatedCar.Color;
            existingCar.Supplier = updatedCar.Supplier;
            existingCar.RatePerDay = updatedCar.RatePerDay;
            existingCar.Seats = updatedCar.Seats;
            existingCar.Luggage = updatedCar.Luggage;
            existingCar.Transmission = updatedCar.Transmission;
            existingCar.FuelType = updatedCar.FuelType;
            existingCar.HasAC = updatedCar.HasAC;
            existingCar.HasCarPlay = updatedCar.HasCarPlay;
            existingCar.Status = updatedCar.Status;
            // existingCar.IsAvailable = updatedCar.IsAvailable;

            // Handle optional new image
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await ImageFile.CopyToAsync(stream);

                existingCar.ImagePath = "/images/" + uniqueFileName;
            }

            _context.Update(existingCar);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{existingCar.Make} {existingCar.Model} updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------- DELETE / SOFT DELETE ----------------------
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id && !v.IsRemoved);
            if (vehicle == null) return NotFound();
            return View(vehicle);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null) return NotFound();

            vehicle.IsRemoved = true;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{vehicle.Make} {vehicle.Model} moved to archive.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------- ARCHIVE ----------------------
        public async Task<IActionResult> Archived()
        {
            var archived = await _context.Vehicles.Where(v => v.IsRemoved).ToListAsync();
            return View(archived);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null) return NotFound();

            vehicle.IsRemoved = false;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{vehicle.Make} {vehicle.Model} restored successfully!";
            return RedirectToAction(nameof(Archived));
        }

        // ---------------------- DELETE PERMANENT ----------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermanent(int id)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null) return NotFound();

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{vehicle.Make} {vehicle.Model} permanently deleted.";
            return RedirectToAction(nameof(Archived));
        }

        // ---------------------- HELPER ----------------------
        private async Task<string> SaveImageAsync(IFormFile? imageFile, string fallbackImage)
        {
            if (imageFile == null || imageFile.Length == 0)
                return fallbackImage;

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await imageFile.CopyToAsync(stream);

            return "/images/" + uniqueFileName;
        }
    }
}
