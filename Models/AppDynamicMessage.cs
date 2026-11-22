using System;
using System.ComponentModel.DataAnnotations;

namespace EcommerceStore.Models;

public class AppDynamicMessage
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Tag { get; set; } = string.Empty;

    public string? Content { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
