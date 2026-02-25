using System.ComponentModel.DataAnnotations;

namespace Yuka2Back.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }

    // Préférences alimentaires
    [MaxLength(50)]
    public string? DietType { get; set; } // Omnivore, Vegetarian, Vegan, Pescatarian, GlutenFree, LactoseFree

    [MaxLength(500)]
    public string? Allergies { get; set; } // Comma-separated: Gluten,Lait,Arachides,etc.

    [MaxLength(500)]
    public string? DietaryGoals { get; set; } // Comma-separated: WeightLoss,MuscleGain,ReduceSugar,ReduceSalt,MoreFiber,MoreProtein

    public bool NotificationsEnabled { get; set; } = true;
    public bool DarkModeEnabled { get; set; } = false;

    [MaxLength(10)]
    public string Language { get; set; } = "fr";

    // Sécurité
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    // Statistiques
    public int TotalScans { get; set; } = 0;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<ScanHistory> ScanHistories { get; set; } = new List<ScanHistory>();
    public ICollection<FavoriteProduct> FavoriteProducts { get; set; } = new List<FavoriteProduct>();
}
