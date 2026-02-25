using System.ComponentModel.DataAnnotations;

namespace Yuka2Back.DTOs;

public class RegisterDto
{
    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }
}

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; }
    public UserProfileDto User { get; set; } = null!;
}

public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
    public bool KeepBiometric { get; set; } = false;
}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateProfileDto
{
    [MaxLength(100)]
    public string? Username { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }
}

public class UpdatePreferencesDto
{
    [MaxLength(50)]
    public string? DietType { get; set; }

    [MaxLength(500)]
    public string? Allergies { get; set; }

    [MaxLength(500)]
    public string? DietaryGoals { get; set; }

    public bool? NotificationsEnabled { get; set; }
    public bool? DarkModeEnabled { get; set; }

    [MaxLength(10)]
    public string? Language { get; set; }
}

public class UserProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? DietType { get; set; }
    public string? Allergies { get; set; }
    public string? DietaryGoals { get; set; }
    public bool NotificationsEnabled { get; set; }
    public bool DarkModeEnabled { get; set; }
    public string Language { get; set; } = "fr";
    public bool IsEmailVerified { get; set; }
    public int TotalScans { get; set; }
    public int TotalFavorites { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserStatsDto
{
    public int TotalScans { get; set; }
    public int TotalFavorites { get; set; }
    public int TotalProducts { get; set; }
    public int ScansThisWeek { get; set; }
    public int ScansThisMonth { get; set; }
    public Dictionary<string, int> NutriScoreDistribution { get; set; } = new();
    public double AverageHealthScore { get; set; }
    public DateTime MemberSince { get; set; }
}

public class DeleteAccountDto
{
    [Required]
    public string Password { get; set; } = string.Empty;
}
