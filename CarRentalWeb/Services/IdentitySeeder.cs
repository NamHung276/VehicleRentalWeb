using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using VehicleRentalWeb.Models;

public class IdentitySeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public IdentitySeeder(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration config)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _config = config;
    }

    public async Task SeedAsync()
    {
        // 1️⃣ Ensure roles exist
        string[] roles = { "Admin", "Customer" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
                if (!roleResult.Succeeded)
                {
                    throw new Exception($"Failed to create role '{role}': " +
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
        }

        // 2️⃣ Admin credentials (fallback if config missing)
        string adminEmail = _config["SeedAdmin:Email"] ?? "adminrental@tester.com";
        string adminPassword = _config["SeedAdmin:Password"] ?? "Admin123!";

        // 3️⃣ Ensure admin user exists
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

            var createUser = await _userManager.CreateAsync(admin, adminPassword);

            if (!createUser.Succeeded)
            {
                throw new Exception("Failed to create admin user: " +
                    string.Join(", ", createUser.Errors.Select(e => e.Description)));
            }
        }

        // 4️⃣ Ensure admin is assigned to Admin role
        if (!await _userManager.IsInRoleAsync(admin, "Admin"))
        {
            var roleAssign = await _userManager.AddToRoleAsync(admin, "Admin");

            if (!roleAssign.Succeeded)
            {
                throw new Exception("Failed to assign Admin role: " +
                    string.Join(", ", roleAssign.Errors.Select(e => e.Description)));
            }
        }
    }
}
