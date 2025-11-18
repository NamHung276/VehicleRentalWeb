using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalWeb.Models;

[Authorize(Roles = "Customer")]
public class CustomerPortalController : Controller
{
    private readonly RentalContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomerPortalController(RentalContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        // Safe nullable int check if CustomerId is int? type
        var rentals = await _context.Rentals
            .Include(r => r.Vehicle)
            .Where(r => r.Customer != null && r.Customer.UserId == user.Id) // adapt if CustomerId type differs
            .ToListAsync();

        return View(rentals);
    }

    public async Task<IActionResult> BrowseVehicles()
    {
        var vehicles = await _context.Vehicles
            .Where(v => !v.IsRemoved && v.Status == "Available")
            .ToListAsync();

        return View(vehicles);
    }
}
