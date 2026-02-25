using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yuka2Back.Models;

public class PageView
{
    [Key]
    public int Id { get; set; }

    public int SessionId { get; set; }
    public AppSession Session { get; set; } = null!;

    public int? UserId { get; set; }

    [Required, MaxLength(200)]
    public string PageName { get; set; } = string.Empty;

    public DateTime EnteredAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExitedAt { get; set; }

    [NotMapped]
    public double? DurationSeconds => ExitedAt.HasValue
        ? (ExitedAt.Value - EnteredAt).TotalSeconds
        : null;
}
