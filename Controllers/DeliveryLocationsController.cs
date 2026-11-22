using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EcommerceStore.Data;
using EcommerceStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceStore.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("deliverylocations")]
    public class DeliveryLocationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeliveryLocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var locations = await _context.DeliveryLocations
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return View(locations);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var locations = await _context.DeliveryLocations
                .OrderBy(l => l.Name)
                .Select(l => new
                {
                    l.Id,
                    l.Name,
                    l.Description,
                    l.Latitude,
                    l.Longitude,
                    l.IsEnabled,
                    CreatedAt = l.CreatedAt.ToString("g")
                })
                .ToListAsync();

            return Json(locations);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateLocationInputModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, message = string.Join("; ", errors) });
                }
                TempData["Error"] = "Please fill in all required fields accurately.";
                return RedirectToAction(nameof(Index));
            }

            var location = new DeliveryLocation
            {
                Name = model.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.DeliveryLocations.Add(location);
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    message = $"Location '{location.Name}' added successfully.",
                    location = new
                    {
                        location.Id,
                        location.Name,
                        location.Description,
                        location.Latitude,
                        location.Longitude,
                        location.IsEnabled,
                        CreatedAt = location.CreatedAt.ToString("g")
                    }
                });
            }

            TempData["Message"] = $"Location '{location.Name}' added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("ToggleStatus/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var location = await _context.DeliveryLocations.FindAsync(id);
            if (location == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Location not found." });
                }
                return NotFound();
            }

            location.IsEnabled = !location.IsEnabled;
            await _context.SaveChangesAsync();

            string statusText = location.IsEnabled ? "enabled" : "disabled";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    isEnabled = location.IsEnabled,
                    message = $"Location '{location.Name}' is now {statusText}."
                });
            }

            TempData["Message"] = $"Location '{location.Name}' is now {statusText}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var location = await _context.DeliveryLocations.FindAsync(id);
            if (location == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Location not found." });
                }
                return NotFound();
            }

            string name = location.Name;
            _context.DeliveryLocations.Remove(location);
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = $"Location '{name}' deleted successfully." });
            }

            TempData["Message"] = $"Location '{name}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class CreateLocationInputModel
    {
        [Required(ErrorMessage = "Location name is required.")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Latitude coordinate is required.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude coordinate is required.")]
        public double Longitude { get; set; }
    }
}
