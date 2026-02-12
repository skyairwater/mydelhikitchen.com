using System.ComponentModel.DataAnnotations;

namespace EcommerceStore.Models
{
    public class CheckoutViewModel
    {
        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Entered phone format is not valid. Use (XXX) XXX-XXXX or XXX-XXX-XXXX.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Delivery Address")]
        public string DeliveryAddress { get; set; } = string.Empty;

        public List<CartItemViewModel> Items { get; set; } = new();
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
