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
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Food Delivery Date")]
        public DateTime DeliveryDate { get; set; }

        [Display(Name = "Lunch Menu")]
        public string? Menu { get; set; }

        [Required]
        [Display(Name = "Lunch Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Delivery Charge")]
        [DataType(DataType.Currency)]
        public decimal DeliveryCharge { get; set; } = 0.00m;

        [Display(Name = "Home Delivery")]
        public bool IsHomeDeliveryAvailable { get; set; } = true;

        [Display(Name = "Uptown Delivery")]
        public bool IsUptownDeliveryAvailable { get; set; } = false;

        [Display(Name = "Comments")]
        public string? Comments { get; set; }

        [Display(Name = "Maximum Number of Lunch")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid number greater than 0.")]
        public int? MaxOrders { get; set; }

        public List<int> SelectedDeliveryLocationIds { get; set; } = new List<int>();
        public List<DeliveryLocation> AvailableDeliveryLocations { get; set; } = new List<DeliveryLocation>();

        [Display(Name = "Add Tax to Customers")]
        public bool AddTaxToCustomers { get; set; } = true;

        [Display(Name = "Tax Percentage")]
        [Range(0.0, 100.0, ErrorMessage = "Tax percentage must be between 0% and 100%.")]
        public decimal TaxPercentage { get; set; } = 9.25m;

        public List<FoodMenuItemViewModel> AlaCarteItems { get; set; } = new List<FoodMenuItemViewModel>();
        public List<FoodMenuItemViewModel> BeverageItems { get; set; } = new List<FoodMenuItemViewModel>();
        public List<FoodMenuItemViewModel> Items { get; set; } = new List<FoodMenuItemViewModel>();
    }
}
