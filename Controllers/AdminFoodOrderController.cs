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
            var adminOrders = await _context.AdminFoodOrders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // Fetch counts of customer orders for each admin order
            var counts = await _context.CustomerFoodOrders
                .GroupBy(o => o.AdminFoodOrderId)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count);

            ViewBag.CustomerOrderCounts = counts;

            return View(adminOrders);
        }

        [HttpGet]
        public async Task<IActionResult> CustomerOrders(int id)
        {
            var adminOrder = await _context.AdminFoodOrders.FindAsync(id);
            if (adminOrder == null) return NotFound();

            var orders = await _context.CustomerFoodOrders
                .Include(o => o.AlaCarteItems)
                .Where(o => o.AdminFoodOrderId == id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.AdminOrder = adminOrder;
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

            var locations = await _context.DeliveryLocations
                .Where(l => l.IsEnabled)
                .OrderBy(l => l.Name)
                .ToListAsync();

            var model = new EditFoodOrderViewModel
            {
                Id = order.Id,
                DeliveryDate = order.DeliveryDate.ToLocalTime(),
                Menu = order.Menu,
                Price = order.Price,
                DeliveryCharge = order.DeliveryCharge,
                IsHomeDeliveryAvailable = order.IsHomeDeliveryAvailable,
                IsUptownDeliveryAvailable = order.IsUptownDeliveryAvailable,
                Comments = order.Comments,
                MaxOrders = order.MaxOrders,
                AlaCarteItems = order.Items.Where(i => i.Category != "Beverage").Select(i => new FoodMenuItemViewModel { Name = i.Name, Price = i.Price }).ToList(),
                BeverageItems = order.Items.Where(i => i.Category == "Beverage").Select(i => new FoodMenuItemViewModel { Name = i.Name, Price = i.Price }).ToList(),
                AvailableDeliveryLocations = locations,
                SelectedDeliveryLocationIds = order.DeliveryLocationIds ?? new List<int>(),
                AddTaxToCustomers = order.AddTaxToCustomers,
                TaxPercentage = order.TaxPercentage
            };

            if (!model.AlaCarteItems.Any())
            {
                model.AlaCarteItems.Add(new FoodMenuItemViewModel());
            }

            if (!model.BeverageItems.Any())
            {
                model.BeverageItems.Add(new FoodMenuItemViewModel());
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditFoodOrderViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!model.IsHomeDeliveryAvailable && !model.IsUptownDeliveryAvailable)
            {
                ModelState.AddModelError("IsHomeDeliveryAvailable", "Please select at least one delivery option (Home Delivery or Uptown Delivery).");
            }

            if (ModelState.IsValid)
            {
                var order = await _context.AdminFoodOrders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
                if (order == null) return NotFound();

                order.DeliveryDate = DateTime.SpecifyKind(model.DeliveryDate, DateTimeKind.Utc);
                order.Menu = model.Menu;
                order.Price = model.Price;
                order.DeliveryCharge = model.DeliveryCharge;
                order.IsHomeDeliveryAvailable = model.IsHomeDeliveryAvailable;
                order.IsUptownDeliveryAvailable = model.IsUptownDeliveryAvailable;
                order.Comments = model.Comments;
                order.MaxOrders = model.MaxOrders;
                order.DeliveryLocationIds = model.SelectedDeliveryLocationIds ?? new List<int>();
                order.AddTaxToCustomers = model.AddTaxToCustomers;
                order.TaxPercentage = model.TaxPercentage;

                _context.AdminFoodOrderItems.RemoveRange(order.Items);
                order.Items.Clear();

                if (model.AlaCarteItems != null && model.AlaCarteItems.Any())
                {
                    foreach (var item in model.AlaCarteItems)
                    {
                        if (string.IsNullOrWhiteSpace(item.Name) && !item.Price.HasValue) continue;
                        order.Items.Add(new AdminFoodOrderItem { Name = item.Name, Price = item.Price, Category = "AlaCarte" });
                    }
                }

                if (model.BeverageItems != null && model.BeverageItems.Any())
                {
                    foreach (var item in model.BeverageItems)
                    {
                        if (string.IsNullOrWhiteSpace(item.Name) && !item.Price.HasValue) continue;
                        order.Items.Add(new AdminFoodOrderItem { Name = item.Name, Price = item.Price, Category = "Beverage" });
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Food Order #{id} updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            model.AvailableDeliveryLocations = await _context.DeliveryLocations
                .Where(l => l.IsEnabled)
                .OrderBy(l => l.Name)
                .ToListAsync();

            model.AvailableDeliveryLocations = await _context.DeliveryLocations
                .Where(l => l.IsEnabled)
                .OrderBy(l => l.Name)
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new FoodOrderViewModel
            {
                DeliveryDate = DateTime.Today,
                IsHomeDeliveryAvailable = false,
                IsUptownDeliveryAvailable = true,
                AddTaxToCustomers = true,
                TaxPercentage = 9.25m
            };

            var locations = await _context.DeliveryLocations
                .Where(l => l.IsEnabled)
                .OrderBy(l => l.Name)
                .ToListAsync();

            model.AvailableDeliveryLocations = locations;
            model.SelectedDeliveryLocationIds = locations.Select(l => l.Id).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoodOrderViewModel model)
        {
            if (!model.IsHomeDeliveryAvailable && !model.IsUptownDeliveryAvailable)
            {
                ModelState.AddModelError("IsHomeDeliveryAvailable", "Please select at least one delivery option (Home Delivery or Uptown Delivery).");
            }

            if (ModelState.IsValid)
            {
                var adminFoodOrder = new AdminFoodOrder
                {
                    DeliveryDate = DateTime.SpecifyKind(model.DeliveryDate, DateTimeKind.Utc),
                    Menu = model.Menu,
                    Price = model.Price,
                    DeliveryCharge = model.DeliveryCharge,
                    IsHomeDeliveryAvailable = model.IsHomeDeliveryAvailable,
                    IsUptownDeliveryAvailable = model.IsUptownDeliveryAvailable,
                    Comments = model.Comments,
                    MaxOrders = model.MaxOrders,
                    DeliveryLocationIds = model.SelectedDeliveryLocationIds ?? new List<int>(),
                    AddTaxToCustomers = model.AddTaxToCustomers,
                    TaxPercentage = model.TaxPercentage
                };

                // Process uploaded photos
                if (model.Photos != null && model.Photos.Count > 0)
                {
                    if (model.Photos.Count > 10)
                    {
                        ModelState.AddModelError("Photos", "You can only upload up to 10 photos.");
                        model.AvailableDeliveryLocations = await _context.DeliveryLocations
                .Where(l => l.IsEnabled)
                .OrderBy(l => l.Name)
                .ToListAsync();

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

                // Map food items (A la carte & Beverages)
                if (model.AlaCarteItems != null && model.AlaCarteItems.Any())
                {
                    foreach (var item in model.AlaCarteItems)
                    {
                        if (string.IsNullOrWhiteSpace(item.Name) && !item.Price.HasValue) 
                            continue;

                        adminFoodOrder.Items.Add(new AdminFoodOrderItem
                        {
                            Name = item.Name,
                            Price = item.Price,
                            Category = "AlaCarte"
                        });
                    }
                }

                if (model.BeverageItems != null && model.BeverageItems.Any())
                {
                    foreach (var item in model.BeverageItems)
                    {
                        if (string.IsNullOrWhiteSpace(item.Name) && !item.Price.HasValue) 
                            continue;

                        adminFoodOrder.Items.Add(new AdminFoodOrderItem
                        {
                            Name = item.Name,
                            Price = item.Price,
                            Category = "Beverage"
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

            model.AvailableDeliveryLocations = await _context.DeliveryLocations
                .Where(l => l.IsEnabled)
                .OrderBy(l => l.Name)
                .ToListAsync();

            return View(model);
        }
    }
}
