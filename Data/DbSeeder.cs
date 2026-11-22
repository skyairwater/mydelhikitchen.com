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
        await SeedAppDynamicMessagesAsync(serviceProvider);
        await SeedDeliveryLocationsAsync(serviceProvider);
    }

    public static async Task SeedDeliveryLocationsAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
        if (!await db.DeliveryLocations.AnyAsync())
        {
            db.DeliveryLocations.AddRange(
                new DeliveryLocation
                {
                    Name = "Bank of America Corporate Center",
                    Description = "100 N Tryon St, Charlotte, NC 28202",
                    Latitude = 35.2273,
                    Longitude = -80.8431,
                    IsEnabled = true
                },
                new DeliveryLocation
                {
                    Name = "Duke Energy Plaza / 550 S Tryon",
                    Description = "525 S Tryon St, Charlotte, NC 28202",
                    Latitude = 35.2238,
                    Longitude = -80.8492,
                    IsEnabled = true
                },
                new DeliveryLocation
                {
                    Name = "The Green / Wells Fargo Plaza",
                    Description = "401 S Tryon St, Charlotte, NC 28202",
                    Latitude = 35.2246,
                    Longitude = -80.8475,
                    IsEnabled = true
                }
            );
            await db.SaveChangesAsync();
        }
    }

    public static async Task SeedAppDynamicMessagesAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
        
        var hasGroceriesMsg = await db.AppDynamicMessages.AnyAsync(m => m.Tag == "Groceries message");
        if (!hasGroceriesMsg)
        {
            db.AppDynamicMessages.Add(new AppDynamicMessage
            {
                Tag = "Groceries message",
                Content = "Free delivery for Delhi Kitchen Customers!",
                LastUpdated = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
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
