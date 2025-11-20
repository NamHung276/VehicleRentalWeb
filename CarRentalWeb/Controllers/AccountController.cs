using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VehicleRentalWeb.Models;
using VehicleRentalWeb.ViewModels;

namespace VehicleRentalWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RentalContext _context;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RentalContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        // --------------------------
        // REGISTER
        // --------------------------
        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                DisplayName = vm.DisplayName
            };

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            // Assign Customer role
            await _userManager.AddToRoleAsync(user, "Customer");

            // Customer profile
            var customer = new Customer
            {
                UserId = user.Id,
                Name = vm.DisplayName,
                Email = vm.Email,
                Phone = vm.Phone,
                Address = vm.Address,
                ImagePath = "/images/default_customer.jpg",
                RegistrationType = RegistrationType.Online,
                IsArchived = false
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Auto-login after register
            await _signInManager.SignInAsync(user, false);

            // Redirect to CustomerPortal
            return RedirectToAction("Home", "CustomerPortal");
        }

        // --------------------------
        // LOGIN
        // --------------------------
        [HttpGet]
        public IActionResult Login(string? returnUrl = null) =>
            View(new LoginViewModel { ReturnUrl = returnUrl });


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await _signInManager.PasswordSignInAsync(
                vm.Email,
                vm.Password,
                vm.RememberMe,
                false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(vm);
            }

            // Get user safely
            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError("", "User account could not be loaded.");
                return View(vm);
            }

            // Role-based redirect
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Index", "AdminDashboard");

            if (await _userManager.IsInRoleAsync(user, "Customer"))
                return RedirectToAction("Home", "CustomerPortal");

            // fallback
            return RedirectToAction("Index", "Home");
        }

        // --------------------------
        // LOGOUT
        // --------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();
    }
}
