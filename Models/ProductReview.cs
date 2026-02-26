using System.ComponentModel.DataAnnotations;

namespace Yuka2Back.Models;

public class ProductReview
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required, Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public bool IsApproved { get; set; } = false;
    public bool IsReported { get; set; } = false;

    [MaxLength(500)]
    public string? ModerationNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
