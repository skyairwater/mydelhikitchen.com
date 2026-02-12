using Microsoft.AspNetCore.Identity;

namespace EcommerceStore.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsAdmin { get; set; } = false;
    }
}
