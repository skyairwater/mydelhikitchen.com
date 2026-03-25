using EcommerceStore.Models;

namespace EcommerceStore.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationEmailAsync(Order order);
        Task SendFoodOrderConfirmationEmailAsync(CustomerFoodOrder order);
    }
}
