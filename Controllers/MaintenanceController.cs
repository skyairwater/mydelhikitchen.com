using EcommerceStore.Data;
using EcommerceStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceStore.Controllers;

[Authorize(Policy = "AdminOnly")]
public class MaintenanceController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var maintenanceModes = await context.MaintenanceModes
            .OrderBy(m => m.ControllerName)
            .ThenBy(m => m.ActionName)
            .ToListAsync();
        return View(maintenanceModes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var maintenanceMode = await context.MaintenanceModes.FindAsync(id);
        if (maintenanceMode != null)
        {
            maintenanceMode.IsEnabled = !maintenanceMode.IsEnabled;
            await context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateMessage(int id, string message)
    {
        var maintenanceMode = await context.MaintenanceModes.FindAsync(id);
        if (maintenanceMode != null)
        {
            maintenanceMode.MaintenanceMessage = message;
            await context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
