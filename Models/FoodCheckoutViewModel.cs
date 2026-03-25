using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcommerceStore.Models
{
    public class FoodCheckoutViewModel
    {
        public int AdminFoodOrderId { get; set; }
        
        public int MainQuantity { get; set; }
        
        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string DeliveryAddress { get; set; } = string.Empty;
        
        public Dictionary<int, int> AlaCarteQuantities { get; set; } = new();
    }
}
