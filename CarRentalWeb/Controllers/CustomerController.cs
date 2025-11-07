using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using VehicleRentalWeb.Models;

namespace VehicleRentalWeb.Controllers
{
    public class CustomerController : Controller
    {
        private static List<Customer> customers = new List<Customer>
        {
            new Customer { Id = 1, Name = "John Smith", Email = "john@example.com", Phone = "555-1234", Address = "123 Elm Street", ImagePath = "/images/default_customer.jpg" },
            new Customer { Id = 2, Name = "Emma Brown", Email = "emma@example.com", Phone = "555-5678", Address = "45 Pine Avenue", ImagePath = "/images/default_customer.jpg" }
        };

        public IActionResult Index()
        {
            return View(customers);
        }

        public IActionResult Details(int id)
        {
            var customer = customers.FirstOrDefault(c => c.Id == id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Customer customer)
        {
            customer.Id = customers.Max(c => c.Id) + 1;
            customers.Add(customer);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var customer = customers.FirstOrDefault(c => c.Id == id);
            if (customer != null)
                customers.Remove(customer);

            return RedirectToAction("Index");
        }
    }
}
