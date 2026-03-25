using EcommerceStore.Data;
using EcommerceStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Index()
        {
            var orders = await _context.AdminFoodOrders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.AdminFoodOrders.FindAsync(id);
            if (order != null)
            {
                order.IsCancelled = true;
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Food Order #{id} has been cancelled.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.AdminFoodOrders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            var model = new EditFoodOrderViewModel
            {
                Id = order.Id,
                DeliveryDate = order.DeliveryDate.ToLocalTime(),
                Menu = order.Menu,
                Price = order.Price,
                DeliveryCharge = order.DeliveryCharge,
                Comments = order.Comments,
                MaxOrders = order.MaxOrders,
                Items = order.Items.Select(i => new FoodMenuItemViewModel { Name = i.Name, Price = i.Price }).ToList()
            };

            if (!model.Items.Any())
            {
                model.Items.Add(new FoodMenuItemViewModel());
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditFoodOrderViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var order = await _context.AdminFoodOrders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
                if (order == null) return NotFound();

                order.DeliveryDate = DateTime.SpecifyKind(model.DeliveryDate, DateTimeKind.Utc);
                order.Menu = model.Menu;
                order.Price = model.Price;
                order.DeliveryCharge = model.DeliveryCharge;
                order.Comments = model.Comments;
                order.MaxOrders = model.MaxOrders;

                _context.AdminFoodOrderItems.RemoveRange(order.Items);
                order.Items.Clear();

                if (model.Items != null && model.Items.Any())
                {
                    foreach (var item in model.Items)
                    {
                        if (string.IsNullOrWhiteSpace(item.Name) && !item.Price.HasValue) continue;
                        order.Items.Add(new AdminFoodOrderItem { Name = item.Name, Price = item.Price });
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Food Order #{id} updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
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

                // Cancel all previous active orders
                var activeOrders = await _context.AdminFoodOrders.Where(o => !o.IsCancelled).ToListAsync();
                foreach (var activeOrder in activeOrders)
                {
                    activeOrder.IsCancelled = true;
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
