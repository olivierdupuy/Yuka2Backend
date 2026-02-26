using System.ComponentModel.DataAnnotations;

namespace Yuka2Back.DTOs;

// --- Admin Auth DTOs ---

public class AdminLoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AdminAuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; }
    public AdminProfileDto Admin { get; set; } = null!;
}

public class AdminProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// --- Tracking DTOs (sent by mobile app) ---

public class StartSessionDto
{
    [Required, MaxLength(200)]
    public string DeviceId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DeviceModel { get; set; }

    [MaxLength(100)]
    public string? DeviceOS { get; set; }

    [MaxLength(50)]
    public string? AppVersion { get; set; }

    /// <summary>
    /// Optional: the session ID to close when starting a new one (sent by the mobile app).
    /// </summary>
    public int? PreviousSessionId { get; set; }
}

public class EndSessionDto
{
    [Required]
    public int SessionId { get; set; }
}

public class TrackPageViewDto
{
    [Required]
    public int SessionId { get; set; }

    [Required, MaxLength(200)]
    public string PageName { get; set; } = string.Empty;
}

public class TrackEventDto
{
    public int? SessionId { get; set; }

    [Required, MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? EventData { get; set; }

    [MaxLength(200)]
    public string? DeviceId { get; set; }
}

// --- Admin Dashboard DTOs ---

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsersToday { get; set; }
    public int ActiveUsersThisWeek { get; set; }
    public int ActiveUsersThisMonth { get; set; }
    public int TotalScans { get; set; }
    public int TotalProducts { get; set; }
    public double AvgSessionDurationSeconds { get; set; }
    public List<MostScannedProductDto> MostScannedProducts { get; set; } = new();
    public List<MostActiveUserDto> MostActiveUsers { get; set; } = new();
    public List<HourlyActivityDto> HourlyActivity { get; set; } = new();
}

public class MostScannedProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public int ScanCount { get; set; }
}

public class MostActiveUserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int EventCount { get; set; }
}

public class HourlyActivityDto
{
    public int Hour { get; set; }
    public int EventCount { get; set; }
}

public class OverviewStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalScans { get; set; }
    public int TotalSessions { get; set; }
    public int TotalEvents { get; set; }
    public int ActiveSessionsNow { get; set; }
    public int NewUsersToday { get; set; }
    public int ScansToday { get; set; }
}

// --- Pagination ---

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

// --- Admin User List DTOs ---

public class AdminUserListDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int TotalScans { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
}

public class AdminUserDetailDto : AdminUserListDto
{
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? DietType { get; set; }
    public string? Allergies { get; set; }
    public string? DietaryGoals { get; set; }
    public bool NotificationsEnabled { get; set; }
    public bool DarkModeEnabled { get; set; }
    public string Language { get; set; } = "fr";
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public int TotalFavorites { get; set; }
    public int TotalSessions { get; set; }
    public int TotalEvents { get; set; }
    public int ScansThisWeek { get; set; }
    public int ScansThisMonth { get; set; }
    public List<RecentActivityDto> RecentActivity { get; set; } = new();
    public List<UserSessionDto> RecentSessions { get; set; } = new();
}

public class UserSessionDto
{
    public int Id { get; set; }
    public string? DeviceModel { get; set; }
    public string? DeviceOS { get; set; }
    public string? AppVersion { get; set; }
    public string? IpAddress { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool IsActive { get; set; }
}

public class RecentActivityDto
{
    public string EventType { get; set; } = string.Empty;
    public string? EventData { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
}

// --- Product Admin DTOs ---

public class AdminProductListDto
{
    public int Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? NutriScore { get; set; }
    public int? HealthScore { get; set; }
    public int ScanCount { get; set; }
    public int FavoriteCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductAnalyticsDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public int TotalScans { get; set; }
    public int TotalViews { get; set; }
    public int TotalFavorites { get; set; }
    public int UniqueUsers { get; set; }
    public List<DailyCountDto> DailyScans { get; set; } = new();
}

public class DailyCountDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

// --- Session DTOs ---

public class SessionListDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string? DeviceModel { get; set; }
    public string? DeviceOS { get; set; }
    public string? AppVersion { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool IsActive { get; set; }
    public string? IpAddress { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public int PageViewCount { get; set; }
    public int EventCount { get; set; }
}

// --- Event DTOs ---

public class EventListDto
{
    public int Id { get; set; }
    public int? SessionId { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? EventData { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceId { get; set; }
}

// --- PageView DTOs ---

public class PageViewListDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int? UserId { get; set; }
    public string PageName { get; set; } = string.Empty;
    public DateTime EnteredAt { get; set; }
    public DateTime? ExitedAt { get; set; }
    public double? DurationSeconds { get; set; }
}

// --- Device Stats ---

public class DeviceStatsDto
{
    public List<DeviceDistributionDto> OSDistribution { get; set; } = new();
    public List<DeviceDistributionDto> DeviceModels { get; set; } = new();
    public List<DeviceDistributionDto> AppVersions { get; set; } = new();
}

public class DeviceDistributionDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

// --- Retention ---

public class RetentionDataDto
{
    public List<DailyCountDto> DailyActiveUsers { get; set; } = new();
    public List<WeeklyCountDto> WeeklyActiveUsers { get; set; } = new();
    public List<MonthlyCountDto> MonthlyActiveUsers { get; set; } = new();
}

public class WeeklyCountDto
{
    public int Year { get; set; }
    public int Week { get; set; }
    public int Count { get; set; }
}

public class MonthlyCountDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
}

// --- Real-time Stats ---

public class RealTimeStatsDto
{
    public int ActiveSessionsNow { get; set; }
    public int PageViewsLast5Min { get; set; }
    public int EventsLast5Min { get; set; }
    public int ScansLast5Min { get; set; }
    public int ActiveUsersNow { get; set; }
    public List<string> RecentPages { get; set; } = new();
    public List<RealtimeEventDto> RecentEvents { get; set; } = new();
    public List<EventTypeCountDto> EventTypeCounts { get; set; } = new();
}

public class RealtimeEventDto
{
    public string EventType { get; set; } = string.Empty;
    public string? EventData { get; set; }
    public string? Username { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EventTypeCountDto
{
    public string EventType { get; set; } = string.Empty;
    public int Count { get; set; }
}

// --- Search Queries ---

public class TopSearchQueryDto
{
    public string Query { get; set; } = string.Empty;
    public int Count { get; set; }
}

// --- Analytics Summary ---

public class AnalyticsSummaryDto
{
    public int TotalEvents { get; set; }
    public int TotalPageViews { get; set; }
    public int TotalSessions { get; set; }
    public int TotalSearches { get; set; }
    public int EventsToday { get; set; }
    public int PageViewsToday { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }
    public double AvgEventsPerUser { get; set; }
    public double AvgSessionDuration { get; set; }
}

public class EventTrendDto
{
    public DateTime Date { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class EventTrendsResponseDto
{
    public List<EventTrendDto> Trends { get; set; } = new();
    public List<string> EventTypes { get; set; } = new();
    public List<DailyCountDto> DailyTotals { get; set; } = new();
}

public class UserGrowthDto
{
    public List<DailyCountDto> DailyRegistrations { get; set; } = new();
    public int TotalNewUsers { get; set; }
    public double AvgPerDay { get; set; }
}

public class PeakHoursDto
{
    public List<HourlyDayDto> Data { get; set; } = new();
}

public class HourlyDayDto
{
    public int DayOfWeek { get; set; }
    public int Hour { get; set; }
    public int Count { get; set; }
}

public class FunnelStatsDto
{
    public int TotalUsers { get; set; }
    public int UsersWithSessions { get; set; }
    public int UsersWithScans { get; set; }
    public int UsersWithFavorites { get; set; }
    public int UsersWithSearches { get; set; }
}

// --- App Config DTOs ---

public class AppConfigDto
{
    public bool MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public string? MinAppVersion { get; set; }
    public string? LatestAppVersion { get; set; }
    public string? ForceUpdateMessage { get; set; }
    public int SessionTimeoutMinutes { get; set; }
    public bool ScanEnabled { get; set; }
    public bool SearchEnabled { get; set; }
    public bool FavoritesEnabled { get; set; }
    public bool RegistrationEnabled { get; set; }
    public bool HistoryEnabled { get; set; }
    public bool OpenFoodFactsEnabled { get; set; }
    public bool BiometricAuthEnabled { get; set; }
    public bool AccountDeletionEnabled { get; set; }
    public bool PasswordResetEnabled { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateAppConfigDto
{
    public bool MaintenanceMode { get; set; }

    [MaxLength(500)]
    public string? MaintenanceMessage { get; set; }

    [MaxLength(20)]
    public string? MinAppVersion { get; set; }

    [MaxLength(20)]
    public string? LatestAppVersion { get; set; }

    [MaxLength(500)]
    public string? ForceUpdateMessage { get; set; }

    [Range(1, 1440)]
    public int SessionTimeoutMinutes { get; set; } = 30;

    public bool ScanEnabled { get; set; } = true;
    public bool SearchEnabled { get; set; } = true;
    public bool FavoritesEnabled { get; set; } = true;
    public bool RegistrationEnabled { get; set; } = true;
    public bool HistoryEnabled { get; set; } = true;
    public bool OpenFoodFactsEnabled { get; set; } = true;
    public bool BiometricAuthEnabled { get; set; } = true;
    public bool AccountDeletionEnabled { get; set; } = true;
    public bool PasswordResetEnabled { get; set; } = true;
}

public class EndSessionsDto
{
    public int? UserId { get; set; }
    public string? DeviceId { get; set; }
}

// --- API Log DTOs ---

public class ApiLogListDto
{
    public int Id { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public int? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}
