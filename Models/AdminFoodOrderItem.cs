using System.ComponentModel.DataAnnotations;

namespace EcommerceStore.Models
{
    public class AdminFoodOrderItem
    {
        public int Id { get; set; }
        
        public int AdminFoodOrderId { get; set; }
        public AdminFoodOrder? AdminFoodOrder { get; set; }

        [MaxLength(500)]
        public string? Name { get; set; }

        public decimal? Price { get; set; }
    }
}
