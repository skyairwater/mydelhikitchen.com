using Microsoft.AspNetCore.Identity;
using EcommerceStore.Data;
using EcommerceStore.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceStore.Data;

public static class DbSeeder
{

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        await SeedAdminUserAsync(serviceProvider);
        // Add other seeding logic here if needed (e.g. categories)
    }

    public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var adminEmail = configuration["AdminUser:Email"];
        var adminPassword = configuration["AdminUser:Password"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            return; // Skip if configuration is not provided
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsAdmin = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else if (!adminUser.IsAdmin)
        {
            adminUser.IsAdmin = true;
            await userManager.UpdateAsync(adminUser);
        }
    }
}
