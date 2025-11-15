using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleRentalWeb.Models;

public class AuthController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // LOGIN PAGE
    public IActionResult Login() => View();

    // LOGIN POST
    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(email);

        // Check for null and password
        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
        {
            ViewBag.Error = "Invalid login.";
            return View();
        }

        // Get user roles (Identity stores roles separately)
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email ?? "unknown@example.com"),
            new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? "Customer")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(principal);

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login");
    }

    public IActionResult Denied() => View();
}
