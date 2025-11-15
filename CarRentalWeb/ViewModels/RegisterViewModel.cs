using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using VehicleRentalWeb.Models;

public class IdentitySeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public IdentitySeeder(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _config = config;
    }

    public async Task SeedAsync()
    {
        // Roles
        var roles = new[] { "Admin", "Customer" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }

        // Default admin - read from configuration or fallback
        var adminEmail = _config["SeedAdmin:Email"] ?? "admin@rental.com";
        var adminPassword = _config["SeedAdmin:Password"] ?? "Admin123!";

        var admin = await _userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                DisplayName = "Administrator"
            };
            var result = await _userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
