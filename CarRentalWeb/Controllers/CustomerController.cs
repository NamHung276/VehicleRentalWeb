using Microsoft.AspNetCore.Mvc;
using VehicleRentalWeb.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleRentalWeb.Controllers
{
    public class CustomerController : Controller
    {
        private readonly RentalContext _context;

        public CustomerController(RentalContext context)
        {
            _context = context;
        }

        // GET: /Customer
        public IActionResult Index()
        {
            var customers = _context.Customers.ToList();
            return View(customers);
        }

        // GET: /Customer/Details/5Microsoft.Data.SqlClient.SqlException: 'Invalid column name 'IsArchived'.'
        public IActionResult Details(int id)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // GET: /Customer/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Customer/Create
        [HttpPost]
        public async Task<IActionResult> Create(Customer customer, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                customer.ImagePath = await SaveCustomerImageAsync(ImageFile);
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Customer {customer.Name} created successfully.";
                return RedirectToAction("Index");
            }

            return View(customer);
        }

// ---------------------- EDIT ----------------------
        // GET: /Customer/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // POST: /Customer/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(Customer customer, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
                return View(customer);

            var existingCustomer = _context.Customers.FirstOrDefault(c => c.Id == customer.Id);
            if (existingCustomer == null)
                return NotFound();

            existingCustomer.Name = customer.Name;
            existingCustomer.Email = customer.Email;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Address = customer.Address;

            // Handle image update
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/customers");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                existingCustomer.ImagePath = "/images/customers/" + uniqueFileName;
            }

            _context.Customers.Update(existingCustomer);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{existingCustomer.Name} updated successfully!";
            return RedirectToAction("Index");
        }

        // GET: /Customer/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // POST: /Customer/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                // Add TempData success message
                TempData["Message"] = $"Customer '{customer.Name}' was successfully deleted!";
            }

            return RedirectToAction("Index");
        }


        // Helper: Save uploaded images
        private async Task<string> SaveCustomerImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return "/images/default_customer.jpg";

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/customers");
            Directory.CreateDirectory(uploads);

            var filename = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(uploads, filename);

            using (var stream = new FileStream(path, FileMode.Create))
                await file.CopyToAsync(stream);

            return "/images/customers/" + filename;
        }
        // ---------------------- ARCHIVE / UNARCHIVE ----------------------
        // POST: /Customer/Archive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                customer.IsArchived = true;
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Customer '{customer.Name}' archived successfully!";
            }
            return RedirectToAction("Index");
        }

        // POST: /Customer/Unarchive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unarchive(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                customer.IsArchived = false;
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Customer '{customer.Name}' restored from archive!";
            }
            return RedirectToAction("Index");
        }

    }
}
