using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcommerceStore.Models
{
    public class CustomerFoodOrder
    {
        public int Id { get; set; }
        public string UniqueOrderId { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        public int AdminFoodOrderId { get; set; }
        public AdminFoodOrder? AdminFoodOrder { get; set; }

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string DeliveryAddress { get; set; } = string.Empty;
        
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Main Order specifics
        public int MainQuantity { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DeliveryCharge { get; set; }

        public List<CustomerFoodOrderItem> AlaCarteItems { get; set; } = new();
    }
}
