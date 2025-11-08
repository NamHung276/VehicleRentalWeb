using Microsoft.AspNetCore.Mvc;
using VehicleRentalWeb.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace VehicleRentalWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly RentalContext _context;

        public AccountController(RentalContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Check if username is taken
                if (_context.Set<User>().Any(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("", "Username already exists.");
                    return View(user);
                }

                // Save the user
                _context.Add(user);
                _context.SaveChanges();

                // Create linked customer profile if user is a normal customer
                if (user.Role == "Customer")
                {
                    var newCustomer = new Customer
                    {
                        Name = user.Username,
                        Email = $"{user.Username}@example.com",
                        RegistrationType = RegistrationType.Online,
                        UserId = user.Id
                    };
                    _context.Add(newCustomer);
                    _context.SaveChanges();
                }

                TempData["Success"] = "Registration successful! You can now log in.";
                return RedirectToAction("Login");
            }
            return View(user);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Set<User>()
                .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // Save session info
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                if (user.Role == "Admin")
                    return RedirectToAction("Index", "AdminDashboard");
                else
                    return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
