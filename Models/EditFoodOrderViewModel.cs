using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcommerceStore.Models
{
    public class EditFoodOrderViewModel
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Food Delivery Date")]
        public DateTime DeliveryDate { get; set; }

        public string? Menu { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Delivery Charge")]
        [DataType(DataType.Currency)]
        public decimal DeliveryCharge { get; set; } = 2.00m;

        [Display(Name = "Comments")]
        public string? Comments { get; set; }

        [Display(Name = "Maximum Number of Orders")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid number greater than 0.")]
        public int? MaxOrders { get; set; }

        public List<FoodMenuItemViewModel> Items { get; set; } = new List<FoodMenuItemViewModel>();
    }
}
