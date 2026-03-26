using System.ComponentModel.DataAnnotations;

namespace EcommerceStore.Models;

public class MaintenanceMode
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ControllerName { get; set; } = string.Empty;

    public string? ActionName { get; set; }

    public bool IsEnabled { get; set; } = true;

    public string MaintenanceMessage { get; set; } = "This functionality is not available at the moment.";
}
