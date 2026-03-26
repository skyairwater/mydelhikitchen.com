using EcommerceStore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace EcommerceStore.Filters;

public class MaintenanceModeFilter(ApplicationDbContext dbContext) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controllerName = context.RouteData.Values["controller"]?.ToString();
        var actionName = context.RouteData.Values["action"]?.ToString();

        // Skip maintenance check for Maintenance controller and Login/Logout actions
        if (controllerName == "Maintenance" || controllerName == "Account" || (controllerName == "Home" && actionName == "Error"))
        {
            await next();
            return;
        }

        // Allow admins to bypass maintenance mode
        if (context.HttpContext.User.HasClaim(c => c.Type == "IsAdmin" && c.Value == "True"))
        {
            await next();
            return;
        }

        if (!string.IsNullOrEmpty(controllerName))
        {
            // Check for record matching both controller and action (granular)
            var maintenanceMode = await dbContext.MaintenanceModes
                .FirstOrDefaultAsync(m => m.ControllerName == controllerName && m.ActionName == actionName);

            // If not found, check for record matching only controller (broad/entire page)
            if (maintenanceMode == null)
            {
                maintenanceMode = await dbContext.MaintenanceModes
                    .FirstOrDefaultAsync(m => m.ControllerName == controllerName && m.ActionName == null);
            }

            if (maintenanceMode != null && !maintenanceMode.IsEnabled)
            {
                context.Result = new ViewResult
                {
                    ViewName = "Maintenance",
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<string>(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
                    {
                        Model = maintenanceMode.MaintenanceMessage
                    }
                };
                return;
            }
        }

        await next();
    }
}
