using System.ComponentModel.DataAnnotations;

namespace EcommerceStore.Models
{
    public enum OrderStatus
    {
        Pending,
        Delivered,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }
        public string UniqueOrderId { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        
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
        
        public string? DeliveryDescription { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
