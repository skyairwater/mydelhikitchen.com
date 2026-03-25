using EcommerceStore.Data;
using EcommerceStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace EcommerceStore.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminFoodOrderController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public AdminFoodOrderController(IWebHostEnvironment env, ApplicationDbContext context)
        {
            _env = env;
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new FoodOrderViewModel
            {
                DeliveryDate = DateTime.Today
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoodOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var adminFoodOrder = new AdminFoodOrder
                {
                    DeliveryDate = DateTime.SpecifyKind(model.DeliveryDate, DateTimeKind.Utc),
                    Menu = model.Menu,
                    Price = model.Price,
                    DeliveryCharge = model.DeliveryCharge,
                    Comments = model.Comments,
                    MaxOrders = model.MaxOrders
                };

                // Process uploaded photos
                if (model.Photos != null && model.Photos.Count > 0)
                {
                    if (model.Photos.Count > 10)
                    {
                        ModelState.AddModelError("Photos", "You can only upload up to 10 photos.");
                        return View(model);
                    }

                    string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "foodorders");
                    Directory.CreateDirectory(uploadsFolder);

                    foreach (var file in model.Photos)
                    {
                        if (file.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + ".jpg";
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using var image = await Image.LoadAsync(file.OpenReadStream());
                            
                            // Resize to max 300x300 while preserving aspect ratio
                            image.Mutate(x => x.Resize(new ResizeOptions
                            {
                                Size = new Size(300, 300),
                                Mode = ResizeMode.Max
                            }));

                            // Save with compression logic (JpegEncoder standard 75 quality)
                            await image.SaveAsync(filePath, new JpegEncoder { Quality = 75 });

                            adminFoodOrder.PhotoPaths.Add($"/images/foodorders/{uniqueFileName}");
                        }
                    }
                }

                // Map food items
                if (model.Items != null && model.Items.Any())
                {
                    foreach (var item in model.Items)
                    {
                        // Skip completely empty rows
                        if (string.IsNullOrWhiteSpace(item.Name) && !item.Price.HasValue) 
                            continue;

                        adminFoodOrder.Items.Add(new AdminFoodOrderItem
                        {
                            Name = item.Name,
                            Price = item.Price
                        });
                    }
                }

                _context.AdminFoodOrders.Add(adminFoodOrder);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Food Order created and saved successfully.";
                return RedirectToAction(nameof(Create));
            }

            return View(model);
        }
    }
}
