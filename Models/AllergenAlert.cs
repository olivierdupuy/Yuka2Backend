using System.ComponentModel.DataAnnotations;

namespace Yuka2Back.Models;

public class AllergenAlert
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required, MaxLength(500)]
    public string MatchedAllergens { get; set; } = string.Empty; // Comma-separated

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
