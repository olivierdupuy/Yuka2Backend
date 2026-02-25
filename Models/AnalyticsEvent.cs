using System.ComponentModel.DataAnnotations;

namespace Yuka2Back.Models;

public class AnalyticsEvent
{
    [Key]
    public int Id { get; set; }

    public int? SessionId { get; set; }
    public AppSession? Session { get; set; }

    public int? UserId { get; set; }

    [Required, MaxLength(100)]
    public string EventType { get; set; } = string.Empty;
    // "scan", "search", "favorite_add", "favorite_remove", "login", "logout",
    // "register", "product_view", "share", "filter_change"

    [MaxLength(4000)]
    public string? EventData { get; set; } // JSON string

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(200)]
    public string? DeviceId { get; set; }
}
