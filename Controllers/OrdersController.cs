using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceStore.Data;
using EcommerceStore.Models;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceStore.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (ModelState.IsValid && model.Items != null && model.Items.Any())
            {
                var order = new Order
                {
                    CustomerEmail = model.CustomerEmail,
                    PhoneNumber = model.PhoneNumber,
                    DeliveryAddress = model.DeliveryAddress,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending
                };

                decimal total = 0;
                foreach (var item in model.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        var orderItem = new OrderItem
                        {
                            ProductId = product.Id,
                            Quantity = item.Quantity,
                            Price = product.Price
                        };
                        order.OrderItems.Add(orderItem);
                        total += product.Price * item.Quantity;
                    }
                }
                order.TotalAmount = total;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Mock Email Sending
                System.Diagnostics.Debug.WriteLine($"Email sent to {order.CustomerEmail} with Order ID: {order.UniqueOrderId}");
                
                return RedirectToAction(nameof(Confirmation), new { id = order.UniqueOrderId });
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Confirmation(string id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.UniqueOrderId == id);
            if (order == null) return NotFound();
            return View(order);
        }

        public async Task<IActionResult> Manage(string id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.UniqueOrderId == id);

            if (order == null) return NotFound();
 
            bool canEdit = (DateTime.UtcNow - order.OrderDate).TotalHours < 12 && order.Status == OrderStatus.Pending;
            ViewBag.CanEdit = canEdit;

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(string id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.UniqueOrderId == id);
            if (order == null) return NotFound();

            if ((DateTime.UtcNow - order.OrderDate).TotalHours < 12)
            {
                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["Message"] = "Order cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "Cancellation window (12 hours) has passed.";
            }

            return RedirectToAction(nameof(Manage), new { id = order.UniqueOrderId });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, CheckoutViewModel model)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UniqueOrderId == id);

            if (order == null) return NotFound();

            if ((DateTime.UtcNow - order.OrderDate).TotalHours >= 12 || order.Status == OrderStatus.Cancelled)
            {
                TempData["Error"] = "Modification window has passed or order is cancelled.";
                return RedirectToAction(nameof(Manage), new { id = order.UniqueOrderId });
            }

            // Remove existing items
            _context.OrderItems.RemoveRange(order.OrderItems);

            // Add new items
            decimal total = 0;
            foreach (var item in model.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null && item.Quantity > 0)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        Price = product.Price
                    };
                    _context.OrderItems.Add(orderItem);
                    total += product.Price * item.Quantity;
                }
            }
            order.TotalAmount = total;
            order.PhoneNumber = model.PhoneNumber;
            order.DeliveryAddress = model.DeliveryAddress;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order updated successfully.";
            return RedirectToAction(nameof(Manage), new { id = order.UniqueOrderId });
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AdminIndex(int page = 1)
        {
            int pageSize = 50;
            var ordersQuery = _context.Orders.OrderByDescending(o => o.OrderDate);
            
            var totalOrders = await ordersQuery.CountAsync();
            var orders = await ordersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            return View(orders);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> MarkAsDelivered(string id, string? description)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.UniqueOrderId == id);
            if (order == null) return NotFound();

            order.Status = OrderStatus.Delivered;
            order.DeliveryDescription = description;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order marked as delivered.";
            return RedirectToAction(nameof(AdminIndex));
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> AdminCancel(string id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.UniqueOrderId == id);
            if (order == null) return NotFound();

            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order cancelled by admin.";
            return RedirectToAction(nameof(AdminIndex));
        }
    }
}
