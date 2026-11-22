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
        public decimal DeliveryCharge { get; set; } = 0.00m;
        public List<string> PhotoPaths { get; set; } = new List<string>();
        public List<AdminFoodOrderItem> Items { get; set; } = new List<AdminFoodOrderItem>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsCancelled { get; set; } = false;
        public bool IsHomeDeliveryAvailable { get; set; } = false;
        public bool IsUptownDeliveryAvailable { get; set; } = true;
        public List<int> DeliveryLocationIds { get; set; } = new List<int>();
        public bool AddTaxToCustomers { get; set; } = true;
        public decimal TaxPercentage { get; set; } = 9.25m;
    }
}
