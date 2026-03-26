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
        await SeedMaintenanceModesAsync(serviceProvider);
    }

    public static async Task SeedMaintenanceModesAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
        
        var pages = new[]
        {
            new { Controller = "Food", Action = (string?)null, Name = "Food (All Pages)" },
            new { Controller = "Home", Action = "Groceries", Name = "Groceries" },
            new { Controller = "Home", Action = "Catering", Name = "Catering" }
        };

        foreach (var page in pages)
        {
            if (!await db.MaintenanceModes.AnyAsync(m => m.ControllerName == page.Controller && m.ActionName == page.Action))
            {
                db.MaintenanceModes.Add(new MaintenanceMode 
                { 
                    ControllerName = page.Controller, 
                    ActionName = page.Action,
                    IsEnabled = true 
                });
            }
        }

        // Clean up old broad maintenance modes if they exist
        var allModes = await db.MaintenanceModes.ToListAsync();
        var oldModes = allModes
            .Where(m => !pages.Any(p => p.Controller == m.ControllerName && p.Action == m.ActionName))
            .ToList();
        
        if (oldModes.Any())
        {
            db.MaintenanceModes.RemoveRange(oldModes);
        }

        await db.SaveChangesAsync();
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
