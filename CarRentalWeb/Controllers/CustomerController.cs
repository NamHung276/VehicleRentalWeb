using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VehicleRentalWeb.Models;

namespace VehicleRentalWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly RentalContext _context;
        private readonly string _imageFolder = "wwwroot/images/customers";

        public CustomerController(RentalContext context)
        {
            _context = context;
        }

        // ---------- INDEX ----------
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers
                .Where(c => !c.IsArchived)
                .ToListAsync();

            return View(customers);
        }

        // ---------- DETAILS ----------
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // ---------- CREATE (GET) ----------
        public IActionResult Create()
        {
            return View();
        }

        // ---------- CREATE (POST) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
                return View(customer);

            // Image upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                Directory.CreateDirectory(_imageFolder);
                string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                string path = Path.Combine(_imageFolder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await ImageFile.CopyToAsync(stream);

                customer.ImagePath = "/images/customers/" + fileName;
            }
            else
            {
                customer.ImagePath = "/images/default_customer.jpg";
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ---------- EDIT (GET) ----------
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // ---------- EDIT (POST) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer, IFormFile? ImageFile)
        {
            if (id != customer.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(customer);

            var existing = await _context.Customers.FindAsync(id);
            if (existing == null) return NotFound();

            // Update fields
            existing.Name = customer.Name;
            existing.Email = customer.Email;
            existing.Phone = customer.Phone;
            existing.Address = customer.Address;

            // Image update
            if (ImageFile != null && ImageFile.Length > 0)
            {
                Directory.CreateDirectory(_imageFolder);
                string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                string path = Path.Combine(_imageFolder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await ImageFile.CopyToAsync(stream);

                existing.ImagePath = "/images/customers/" + fileName;
            }

            _context.Customers.Update(existing);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ---------- DELETE (ARCHIVE) ----------
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            customer.IsArchived = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ---------- ARCHIVED LIST ----------
        public async Task<IActionResult> Archived()
        {
            var archived = await _context.Customers
                .Where(c => c.IsArchived)
                .ToListAsync();

            return View(archived);
        }

        // ---------- RESTORE ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            customer.IsArchived = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Archived));
        }

        // ---------- PERMANENT DELETE ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermanent(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Archived));
        }
    }
}
