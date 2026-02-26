using System.ComponentModel.DataAnnotations;

namespace Yuka2Back.Models;

/// <summary>
/// Singleton configuration for the mobile app, editable from the admin panel.
/// </summary>
public class AppConfig
{
    public int Id { get; set; }

    // --- Maintenance ---
    public bool MaintenanceMode { get; set; }

    [MaxLength(500)]
    public string? MaintenanceMessage { get; set; }

    // --- Version control ---
    [MaxLength(20)]
    public string? MinAppVersion { get; set; }

    [MaxLength(20)]
    public string? LatestAppVersion { get; set; }

    [MaxLength(500)]
    public string? ForceUpdateMessage { get; set; }

    // --- Session control ---
    public int SessionTimeoutMinutes { get; set; } = 30;

    // --- Feature flags ---
    public bool ScanEnabled { get; set; } = true;
    public bool SearchEnabled { get; set; } = true;
    public bool FavoritesEnabled { get; set; } = true;
    public bool RegistrationEnabled { get; set; } = true;
    public bool HistoryEnabled { get; set; } = true;
    public bool OpenFoodFactsEnabled { get; set; } = true;
    public bool BiometricAuthEnabled { get; set; } = true;
    public bool AccountDeletionEnabled { get; set; } = true;
    public bool PasswordResetEnabled { get; set; } = true;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
