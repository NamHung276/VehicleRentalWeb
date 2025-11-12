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

        // ---------------------- CREATE (GET) ----------------------
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.VehicleTypes = new List<string> { "Car", "Bike", "Truck" };
            ViewBag.FuelTypes = new List<string> { "Fuel", "Diesel", "Electric", "Hybrid" };
            ViewBag.Categories = new List<string> { "Sedan", "SUV", "Hatchback", "Coupe" };
            return View();
        }

        // ---------------------- CREATE (POST) ----------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string vehicleType,
            [Bind("Make,Model,Year,Color,Supplier,RatePerDay")] Car formVehicle,
            IFormFile? ImageFile,
            int? Seats,
            string? FuelType,
            string? Category,
            int? EngineCC,
            bool? IsElectric,
            double? CapacityInTons)
        {
            Vehicle newVehicle;

            switch (vehicleType)
            {
                case "Car":
                    newVehicle = new Car
                    {
                        Make = formVehicle.Make,
                        Model = formVehicle.Model,
                        Year = formVehicle.Year,
                        Color = formVehicle.Color,
                        Supplier = formVehicle.Supplier,
                        RatePerDay = formVehicle.RatePerDay,
                        Seats = Seats ?? 4,
                        FuelType = FuelType ?? "Fuel",
                        Category = Category ?? "Sedan",
                        Status = "Available"
                    };
                    break;

                case "Bike":
                    newVehicle = new Bike
                    {
                        Make = formVehicle.Make,
                        Model = formVehicle.Model,
                        Year = formVehicle.Year,
                        Color = formVehicle.Color,
                        Supplier = formVehicle.Supplier,
                        RatePerDay = formVehicle.RatePerDay,
                        EngineCC = EngineCC ?? 125,
                        IsElectric = IsElectric ?? false,
                        Status = "Available"
                    };
                    break;

                case "Truck":
                    newVehicle = new Truck
                    {
                        Make = formVehicle.Make,
                        Model = formVehicle.Model,
                        Year = formVehicle.Year,
                        Color = formVehicle.Color,
                        Supplier = formVehicle.Supplier,
                        RatePerDay = formVehicle.RatePerDay,
                        CapacityInTons = CapacityInTons ?? 1.0,
                        Status = "Available"
                    };
                    break;

                default:
                    ModelState.AddModelError("", "Invalid vehicle type selected.");
                    ViewBag.VehicleTypes = new List<string> { "Car", "Bike", "Truck" };
                    return View();
            }

            // ---------------------- Image Upload ----------------------
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await ImageFile.CopyToAsync(stream);
                newVehicle.ImagePath = "/images/" + uniqueFileName;
            }
            else
            {
                newVehicle.ImagePath = "/images/default_car.jpg";
            }

            // ---------------------- Save to Database ----------------------
            newVehicle.IsRemoved = false;

            _context.Vehicles.Add(newVehicle);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{newVehicle.Make} {newVehicle.Model} added successfully!";
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

        // Updated POST Edit: bind from form into a new derived instance using TryUpdateModelAsync
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string vehicleType,
            IFormFile? ImageFile)
        {
            var existing = await _context.Vehicles.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Create a fresh derived instance and populate from the Request.Form
            Vehicle updated;
            if (vehicleType == "Car")
            {
                var temp = new Car();
                // Bind all incoming values that match property names on Car
                await TryUpdateModelAsync(temp, prefix: "");
                updated = temp;
            }
            else if (vehicleType == "Bike")
            {
                var temp = new Bike();
                await TryUpdateModelAsync(temp, prefix: "");
                updated = temp;
            }
            else if (vehicleType == "Truck")
            {
                var temp = new Truck();
                await TryUpdateModelAsync(temp, prefix: "");
                updated = temp;
            }
            else
            {
                updated = existing;
            }

            // Shared properties
            existing.Make = updated.Make;
            existing.Model = updated.Model;
            existing.Year = updated.Year;
            existing.Color = updated.Color;
            existing.Supplier = updated.Supplier;
            existing.RatePerDay = updated.RatePerDay;
            existing.Status = updated.Status;

            // Optional image upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await ImageFile.CopyToAsync(stream);
                existing.ImagePath = "/images/" + uniqueFileName;
            }

            // Type-specific logic
            if (existing is Car existingCar && updated is Car updatedCar)
            {
                existingCar.Seats = updatedCar.Seats;
                existingCar.Luggage = updatedCar.Luggage;
                existingCar.Transmission = updatedCar.Transmission;
                existingCar.FuelType = updatedCar.FuelType;
                existingCar.HasAC = updatedCar.HasAC;
                existingCar.HasCarPlay = updatedCar.HasCarPlay;
            }
            else if (existing is Bike existingBike && updated is Bike updatedBike)
            {
                existingBike.EngineCC = updatedBike.EngineCC;
                existingBike.Transmission = updatedBike.Transmission;
                existingBike.IsElectric = updatedBike.IsElectric;
            }
            else if (existing is Truck existingTruck && updated is Truck updatedTruck)
            {
                existingTruck.CapacityInTons = updatedTruck.CapacityInTons;
                existingTruck.Transmission = updatedTruck.Transmission;
                existingTruck.HasAC = updatedTruck.HasAC;
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = $"{existing.Make} {existing.Model} updated successfully!";
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
