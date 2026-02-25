using System.ComponentModel.DataAnnotations;

namespace Yuka2Back.Models;

public class ApiLog
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(10)]
    public string Method { get; set; } = string.Empty; // GET, POST, PUT, DELETE, etc.

    [Required, MaxLength(500)]
    public string Path { get; set; } = string.Empty;

    public int StatusCode { get; set; }

    public long DurationMs { get; set; }

    public int? UserId { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
