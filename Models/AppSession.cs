using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yuka2Back.Models;

public class AppSession
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [Required, MaxLength(200)]
    public string DeviceId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DeviceModel { get; set; }

    [MaxLength(100)]
    public string? DeviceOS { get; set; }

    [MaxLength(50)]
    public string? AppVersion { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? EndedAt { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [NotMapped]
    public bool IsActive => EndedAt == null;

    // Navigation
    public ICollection<PageView> PageViews { get; set; } = new List<PageView>();
    public ICollection<AnalyticsEvent> Events { get; set; } = new List<AnalyticsEvent>();
}
