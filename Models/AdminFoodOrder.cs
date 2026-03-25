using System;
using System.Collections.Generic;

namespace EcommerceStore.Models
{
    public class AdminFoodOrder
    {
        public int Id { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? Menu { get; set; }
        public decimal Price { get; set; }
        public string? Comments { get; set; }
        public int? MaxOrders { get; set; }
        public decimal DeliveryCharge { get; set; } = 2.00m;
        public List<string> PhotoPaths { get; set; } = new List<string>();
        public List<AdminFoodOrderItem> Items { get; set; } = new List<AdminFoodOrderItem>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsCancelled { get; set; } = false;
    }
}
