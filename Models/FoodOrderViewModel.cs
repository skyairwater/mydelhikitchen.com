using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

using System.Collections.Generic;

namespace EcommerceStore.Models
{
    public class FoodMenuItemViewModel
    {
        [MaxLength(500)]
        public string? Name { get; set; }

        [DataType(DataType.Currency)]
        public decimal? Price { get; set; }
    }

    public class FoodOrderViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Food Delivery Date")]
        public DateTime DeliveryDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Menu")]
        public string Menu { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Delivery Charge")]
        [DataType(DataType.Currency)]
        public decimal DeliveryCharge { get; set; } = 2.00m;

        [Display(Name = "Comments")]
        public string? Comments { get; set; }

        [Display(Name = "Upload Photos (Max 10)")]
        public List<IFormFile>? Photos { get; set; } = new List<IFormFile>();

        [Display(Name = "Maximum Number of Orders")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid number greater than 0.")]
        public int? MaxOrders { get; set; }

        public List<FoodMenuItemViewModel> Items { get; set; } = new List<FoodMenuItemViewModel> { new FoodMenuItemViewModel() };
    }
}
