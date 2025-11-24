using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehicleRentalWeb.Models;

var builder = WebApplication.CreateBuilder(args);

//
// -------------------------------------------------------------
// 1. DATABASE + IDENTITY CONFIG
// -------------------------------------------------------------
builder.Services.AddDbContext<RentalContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<RentalContext>()
.AddDefaultTokenProviders();

// Configure cookie paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});

//
// -------------------------------------------------------------
// 2. MVC, HTTP CLIENT, SWAGGER
// -------------------------------------------------------------
builder.Services.AddControllersWithViews();

// Register IHttpClientFactory (required for AI Chatbot)
builder.Services.AddHttpClient();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//
// -------------------------------------------------------------
// 3. SEED ROLES + ADMIN USER
// -------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var seeder = new VehicleRentalWeb.Seeding.IdentitySeeder(
        services.GetRequiredService<RoleManager<IdentityRole>>(),
        services.GetRequiredService<UserManager<ApplicationUser>>(),
        services.GetRequiredService<IConfiguration>()
    );

    await seeder.SeedAsync();
}

//
// -------------------------------------------------------------
// 4. MIDDLEWARE PIPELINE
// -------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // Must come before UseAuthorization
app.UseAuthorization();

//
// -------------------------------------------------------------
// 5. ROUTING
// -------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapControllers(); // For API endpoints (Chatbot, etc.)

app.Run();
