using EcommerceStore.Data;
using EcommerceStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceStore.Controllers
{
    [Route("Food")]
    public class FoodController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FoodController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var activeOrder = await _context.AdminFoodOrders
                .Include(o => o.Items)
                .Where(o => !o.IsCancelled)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (activeOrder != null && activeOrder.MaxOrders.HasValue)
            {
                var totalOrdered = await _context.CustomerFoodOrders
                    .Where(o => o.AdminFoodOrderId == activeOrder.Id)
                    .CountAsync();
                
                ViewBag.RemainingOrders = activeOrder.MaxOrders.Value - totalOrdered;
            }

            return View(activeOrder);
        }

        [HttpPost("Checkout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(FoodCheckoutViewModel model)
        {
            if (ModelState.IsValid)
            {
                var adminOrder = await _context.AdminFoodOrders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == model.AdminFoodOrderId);
                if (adminOrder == null || adminOrder.IsCancelled) 
                {
                    TempData["Error"] = "This food order is no longer available.";
                    return RedirectToAction(nameof(Index));
                }

                if (adminOrder.MaxOrders.HasValue)
                {
                    var totalOrdered = await _context.CustomerFoodOrders
                        .Where(o => o.AdminFoodOrderId == adminOrder.Id)
                        .CountAsync();
                        
                    if (totalOrdered >= adminOrder.MaxOrders.Value)
                    {
                        TempData["Error"] = $"Sorry, this menu is sold out. No more orders can be placed.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                var customerOrder = new CustomerFoodOrder
                {
                    AdminFoodOrderId = adminOrder.Id,
                    CustomerEmail = model.CustomerEmail,
                    PhoneNumber = model.PhoneNumber,
                    DeliveryAddress = model.DeliveryAddress,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    MainQuantity = model.MainQuantity,
                    BasePrice = adminOrder.Price,
                    DeliveryCharge = adminOrder.DeliveryCharge
                };

                decimal total = (adminOrder.Price * model.MainQuantity) + adminOrder.DeliveryCharge;

                if (model.AlaCarteQuantities != null)
                {
                    foreach (var (itemId, qty) in model.AlaCarteQuantities)
                    {
                        if (qty > 0)
                        {
                            var alaCarteItem = adminOrder.Items.FirstOrDefault(i => i.Id == itemId);
                            if (alaCarteItem != null && alaCarteItem.Price.HasValue)
                            {
                                customerOrder.AlaCarteItems.Add(new CustomerFoodOrderItem
                                {
                                    AdminFoodOrderItemId = alaCarteItem.Id,
                                    Name = alaCarteItem.Name,
                                    Price = alaCarteItem.Price.Value,
                                    Quantity = qty
                                });
                                total += (alaCarteItem.Price.Value * qty);
                            }
                        }
                    }
                }

                customerOrder.TotalAmount = total;
                _context.CustomerFoodOrders.Add(customerOrder);
                await _context.SaveChangesAsync();
                
                TempData["Message"] = "Food Order placed successfully!";
                return RedirectToAction(nameof(Confirmation), new { id = customerOrder.UniqueOrderId });
            }
            TempData["Error"] = "Please fill in all required fields.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Confirmation/{id}")]
        public async Task<IActionResult> Confirmation(string id)
        {
            var customerOrder = await _context.CustomerFoodOrders
                .Include(o => o.AdminFoodOrder)
                .Include(o => o.AlaCarteItems)
                .FirstOrDefaultAsync(o => o.UniqueOrderId == id);

            if (customerOrder == null)
            {
                return NotFound();
            }

            return View(customerOrder);
        }
    }
}
