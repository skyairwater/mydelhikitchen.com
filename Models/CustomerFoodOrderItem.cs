using System.ComponentModel.DataAnnotations;

namespace EcommerceStore.Models
{
    public class CustomerFoodOrderItem
    {
        public int Id { get; set; }
        
        public int CustomerFoodOrderId { get; set; }
        public CustomerFoodOrder? CustomerFoodOrder { get; set; }

        public int AdminFoodOrderItemId { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Name { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
