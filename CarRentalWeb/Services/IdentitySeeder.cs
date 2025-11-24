using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using VehicleRentalWeb.Models;

namespace VehicleRentalWeb.Seeding
{
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
                    var result = await _roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                        throw new Exception("Role creation failed: " +
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // 2️⃣ Admin credentials (fallback if config missing)
            string adminEmail = _config["SeedAdmin:Email"] ?? "adminrental@tester.com";
            string adminPassword = _config["SeedAdmin:Password"] ?? "Admin123!";

            // 3️⃣ Ensure admin exists
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

                var created = await _userManager.CreateAsync(admin, adminPassword);

                if (!created.Succeeded)
                    throw new Exception("Admin user creation failed: " +
                        string.Join(", ", created.Errors.Select(e => e.Description)));
            }

            // 4️⃣ Ensure admin has Admin role
            if (!await _userManager.IsInRoleAsync(admin, "Admin"))
            {
                var assigned = await _userManager.AddToRoleAsync(admin, "Admin");

                if (!assigned.Succeeded)
                    throw new Exception("Assigning Admin role failed: " +
                        string.Join(", ", assigned.Errors.Select(e => e.Description)));
            }
        }
    }
}
