using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceStore.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        public string ImageFileName { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
