using System.ComponentModel.DataAnnotations;

namespace Yuka2Back.Models;

public class ProductComparison
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [Required, MaxLength(200)]
    public string ProductIds { get; set; } = string.Empty; // Comma-separated product IDs

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }
}
