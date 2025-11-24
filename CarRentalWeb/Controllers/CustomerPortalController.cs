using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VehicleRentalWeb.Models;
using VehicleRentalWeb.Models.ViewModels;

namespace VehicleRentalWeb.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerPortalController : Controller
    {
        private readonly RentalContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _openRouterApiKey;
        private readonly IHttpClientFactory _httpClientFactory;

        public CustomerPortalController(
            RentalContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _openRouterApiKey = configuration?["OpenRouter:ApiKey"] ?? string.Empty;
        }

        // ──────────────── HOME ────────────────
        public IActionResult Home() => View();

        // ──────────────── BROWSE VEHICLES ────────────────
        public async Task<IActionResult> BrowseVehicles(
            string? search,
            string? type,
            string? fuel,
            string? trans,
            string? status,
            string? sort)
        {
            IQueryable<Vehicle> query = _context.Vehicles.Where(v => !v.IsRemoved && v.Status == "Available");

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(v => v.Status == status);

            // Filter by type (Car/Bike/Truck)
            if (!string.IsNullOrWhiteSpace(type))
            {
                var t = type.Trim().ToLowerInvariant();
                query = t switch
                {
                    "car" => query.OfType<Car>(),
                    "bike" => query.OfType<Bike>(),
                    "truck" => query.OfType<Truck>(),
                    _ => query
                };
            }

            // Execute DB query
            var vehiclesList = await query.ToListAsync();

            // Subclass-specific filters in-memory
            if (!string.IsNullOrWhiteSpace(trans))
            {
                var transTrim = trans.Trim();
                vehiclesList = vehiclesList
                    .Where(v =>
                        (v is Car car && string.Equals(car.Transmission, transTrim, StringComparison.OrdinalIgnoreCase)) ||
                        (v is Bike bike && string.Equals(bike.Transmission, transTrim, StringComparison.OrdinalIgnoreCase)) ||
                        (v is Truck truck && string.Equals(truck.Transmission, transTrim, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(fuel))
            {
                var fuelTrim = fuel.Trim();
                vehiclesList = vehiclesList
                    .Where(v => v is Car car && string.Equals(car.FuelType, fuelTrim, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Search by Make/Model
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowered = search.Trim().ToLowerInvariant();
                vehiclesList = vehiclesList
                    .Where(v =>
                        (!string.IsNullOrEmpty(v.Make) && v.Make.ToLowerInvariant().Contains(lowered)) ||
                        (!string.IsNullOrEmpty(v.Model) && v.Model.ToLowerInvariant().Contains(lowered)))
                    .ToList();
            }

            // Sorting
            vehiclesList = sort?.Trim() switch
            {
                "price_asc" => vehiclesList.OrderBy(v => v.RatePerDay).ToList(),
                "price_desc" => vehiclesList.OrderByDescending(v => v.RatePerDay).ToList(),
                "year_new" => vehiclesList.OrderByDescending(v => v.Year).ToList(),
                "year_old" => vehiclesList.OrderBy(v => v.Year).ToList(),
                "make_az" => vehiclesList.OrderBy(v => v.Make).ToList(),
                "make_za" => vehiclesList.OrderByDescending(v => v.Make).ToList(),
                _ => vehiclesList
            };

            return View(vehiclesList);
        }

        // ──────────────── VEHICLE DETAILS ────────────────
        public async Task<IActionResult> VehicleDetails(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();
            return View(vehicle);
        }

        // ──────────────── REQUEST RENTAL ────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRental(int vehicleId, DateTime startDate, DateTime endDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null)
            {
                TempData["Error"] = "Customer profile not found.";
                return RedirectToAction(nameof(BrowseVehicles));
            }

            if (endDate <= startDate)
            {
                TempData["Error"] = "End date must be later than start date.";
                return RedirectToAction(nameof(VehicleDetails), new { id = vehicleId });
            }

            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null || vehicle.IsRemoved || vehicle.Status != "Available")
            {
                TempData["Error"] = "Vehicle not available.";
                return RedirectToAction(nameof(BrowseVehicles));
            }

            int days = (int)(endDate - startDate).TotalDays;
            if (days < 1) days = 1;

            var totalCost = vehicle.RatePerDay * days;

            var rental = new Rental
            {
                VehicleId = vehicleId,
                CustomerId = customer.Id,
                StartDate = startDate,
                EndDate = endDate,
                TotalCost = totalCost,
                Status = RentalStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Rental requested — total {totalCost:C}.";
            return RedirectToAction(nameof(MyRentals));
        }

        // ──────────────── MY RENTALS ────────────────
        public async Task<IActionResult> MyRentals()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null)
            {
                TempData["Error"] = "Customer profile not found.";
                return RedirectToAction(nameof(Home));
            }

            var rentals = await _context.Rentals
                .Include(r => r.Vehicle)
                .Where(r => r.CustomerId == customer.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(rentals);
        }

        // ──────────────── PAY RENTAL ────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayRental(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null) return NotFound();

            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == customer.Id);

            if (rental == null) return NotFound();

            if (rental.PaymentStatus == PaymentStatus.Paid)
            {
                TempData["Error"] = "Rental already paid.";
                return RedirectToAction(nameof(MyRentals));
            }

            rental.PaymentStatus = PaymentStatus.Paid;
            rental.PaymentDate = DateTime.Now;
            rental.TransactionId = $"SIM-{Guid.NewGuid().ToString().Split('-')[0].ToUpper()}";

            _context.Rentals.Update(rental);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Payment successful!";
            return RedirectToAction(nameof(MyRentals));
        }

        // ──────────────── PROFILE ────────────────
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);

            var model = new CustomerProfileViewModel
            {
                DisplayName = user.DisplayName ?? "Unknown",
                Email = user.Email ?? "Unknown",
                PhoneNumber = user.PhoneNumber ?? "N/A",
                Address = customer?.Address ?? "No address provided"
            };

            return View(model);
        }

        // ──────────────── EDIT PROFILE ────────────────
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);

            var model = new EditProfileViewModel
            {
                DisplayName = user.DisplayName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Address = customer?.Address ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (model == null) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Update Identity
            user.DisplayName = model.DisplayName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            // Update Customer
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer != null)
            {
                customer.Name = model.DisplayName;
                customer.Email = model.Email;
                customer.Phone = model.PhoneNumber;
                customer.Address = model.Address;

                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
            }

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        // ──────────────── AI CHATBOT ────────────────
        [HttpPost]
        public async Task<IActionResult> ChatBot([FromBody] ChatRequestModel request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Message cannot be empty." });

            if (string.IsNullOrWhiteSpace(_openRouterApiKey))
                return StatusCode(500, new { error = "AI API key not configured." });

            try
            {
                // Fetch all available vehicles from DB (Cars, Bikes, Trucks)
                var vehicles = await _context.Vehicles
                    .Where(v => !v.IsRemoved && v.Status == "Available")
                    .Select(v => new
                    {
                        v.Make,
                        v.Model,
                        v.Year,
                        v.Category,
                        v.RatePerDay,
                        Type = v is Car ? "Car" : v is Bike ? "Bike" : v is Truck ? "Truck" : "Vehicle"
                    })
                    .ToListAsync();

                string vehicleList = vehicles.Count == 0
                    ? "No vehicles currently available."
                    : string.Join("\n", vehicles.Select(v =>
                        $"- {v.Type}: {v.Make} {v.Model} ({v.Year}) at ${v.RatePerDay}/day"));

                // Build system prompt with current vehicles
                string systemPrompt = $"You are a helpful assistant for a vehicle rental website.\n" +
                                      $"Current available vehicles:\n{vehicleList}\n" +
                                      $"Answer user questions about vehicles, bookings, rentals, profiles, or payments clearly and concisely. " +
                                      $"Provide correct vehicle types and daily rates, and do not mention vehicles that are not available.";

                // Call OpenRouter AI
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _openRouterApiKey);

                var payload = new
                {
                    model = "x-ai/grok-4.1-fast:free",
                    messages = new[]
                    {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = request.Message }
            }
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
                if (!response.IsSuccessStatusCode)
                {
                    string raw = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new
                    {
                        error = "AI service returned non-success.",
                        body = raw
                    });
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                // Extract reply from AI response
                string ExtractReply(JsonElement elem)
                {
                    if (elem.TryGetProperty("choices", out var ch) &&
                        ch.ValueKind == JsonValueKind.Array &&
                        ch.GetArrayLength() > 0)
                    {
                        var first = ch[0];
                        if (first.TryGetProperty("message", out var msg) &&
                            msg.TryGetProperty("content", out var msgContent) &&
                            msgContent.ValueKind == JsonValueKind.String)
                            return msgContent.GetString() ?? "";
                        if (first.TryGetProperty("text", out var txt) &&
                            txt.ValueKind == JsonValueKind.String)
                            return txt.GetString() ?? "";
                    }
                    if (elem.TryGetProperty("output", out var output) &&
                        output.ValueKind == JsonValueKind.String)
                        return output.GetString() ?? "";
                    return "";
                }

                string reply = ExtractReply(root);
                if (string.IsNullOrWhiteSpace(reply))
                {
                    var trimmed = responseBody.Length > 2000
                        ? responseBody.Substring(0, 2000) + "..."
                        : responseBody;

                    return StatusCode(500, new
                    {
                        error = "Unexpected AI response format.",
                        response = trimmed
                    });
                }

                return Json(new { reply });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"AI request failed: {ex.Message}" });
            }
        }
    }

    public class ChatRequestModel
    {
        public string Message { get; set; } = string.Empty;
    }
}
