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

    // My rentals / Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (customer == null) return RedirectToAction("Index", "Home"); // or show message

        var rentals = await _context.Rentals
            .Include(r => r.Vehicle)
            .Where(r => r.CustomerId == customer.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(rentals);
    }

    // Browse available vehicles
    public async Task<IActionResult> BrowseVehicles()
    {
        var vehicles = await _context.Vehicles
            .Where(v => !v.IsRemoved && v.Status == "Available")
            .ToListAsync();

        return View(vehicles);
    }

    // Show rent request form (GET)
    [HttpGet]
    public async Task<IActionResult> Rent(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();

        var model = new Rental
        {
            VehicleId = vehicle.Id,
            Vehicle = vehicle,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1),
            RentalFee = 0m
        };

        return View(model);
    }

    // Handle rent request submission (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rent(Rental rental)
    {
        if (!ModelState.IsValid) return View(rental);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (customer == null)
        {
            ModelState.AddModelError("", "Customer profile not found.");
            return View(rental);
        }

        var vehicle = await _context.Vehicles.FindAsync(rental.VehicleId);
        if (vehicle == null)
        {
            ModelState.AddModelError("", "Selected vehicle not found.");
            return View(rental);
        }

        // Calculate fee (min 1 day)
        var days = (int)(rental.EndDate - rental.StartDate).TotalDays;
        if (days < 1) days = 1;
        rental.RentalFee = vehicle.RatePerDay * days;

        // Link customer and set initial status
        rental.CustomerId = customer.Id;
        rental.Status = RentalStatus.Pending;
        rental.CreatedAt = DateTime.Now;

        _context.Rentals.Add(rental);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Rental request sent. Admin will review and approve.";
        return RedirectToAction(nameof(Dashboard));
    }
}
