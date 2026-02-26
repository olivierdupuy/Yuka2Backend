using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Yuka2Back.Data;
using Yuka2Back.DTOs;
using Yuka2Back.Hubs;
using Yuka2Back.Models;
using Yuka2Back.Services;

namespace Yuka2Back.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "SuperAdmin,Admin,Viewer")]
public class AdminController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;
    private readonly AppDbContext _context;
    private readonly IHubContext<AdminHub> _adminHub;

    public AdminController(AnalyticsService analyticsService, AppDbContext context, IHubContext<AdminHub> adminHub)
    {
        _analyticsService = analyticsService;
        _context = context;
        _adminHub = adminHub;
    }

    // ==================== Dashboard ====================

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var stats = await _analyticsService.GetDashboardStats();
        return Ok(stats);
    }

    [HttpGet("stats/overview")]
    public async Task<IActionResult> GetOverview()
    {
        var stats = await _analyticsService.GetOverviewStats();
        return Ok(stats);
    }

    // ==================== Users ====================

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = true,
        [FromQuery] string? status = null)
    {
        var result = await _analyticsService.GetUsers(page, pageSize, search, sortBy, sortDesc, status);
        return Ok(result);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _analyticsService.GetUserAnalytics(id);
        if (user == null) return NotFound(new { message = "User not found." });
        return Ok(user);
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPut("users/{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "User not found." });

        if (dto.IsActive)
        {
            user.LockoutEnd = null;
            user.FailedLoginAttempts = 0;
        }
        else
        {
            // Lock the user indefinitely
            user.LockoutEnd = DateTime.UtcNow.AddYears(100);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = dto.IsActive ? "User activated." : "User deactivated." });
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.ScanHistories)
            .Include(u => u.FavoriteProducts)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound(new { message = "User not found." });

        _context.RemoveRange(user.RefreshTokens);
        _context.RemoveRange(user.ScanHistories);
        _context.RemoveRange(user.FavoriteProducts);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User deleted." });
    }

    // ==================== Products ====================

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var result = await _analyticsService.GetProducts(page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("products/{id}/analytics")]
    public async Task<IActionResult> GetProductAnalytics(int id)
    {
        var analytics = await _analyticsService.GetProductAnalytics(id);
        if (analytics == null) return NotFound(new { message = "Product not found." });
        return Ok(analytics);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("products/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.ScanHistories)
            .Include(p => p.FavoriteProducts)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound(new { message = "Product not found." });

        _context.RemoveRange(product.ScanHistories);
        _context.RemoveRange(product.FavoriteProducts);
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Product deleted." });
    }

    // ==================== Reviews ====================

    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var query = _context.ProductReviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .AsQueryable();

        if (status == "approved") query = query.Where(r => r.IsApproved);
        else if (status == "pending") query = query.Where(r => !r.IsApproved && !r.IsReported);
        else if (status == "reported") query = query.Where(r => r.IsReported);

        var total = await query.CountAsync();

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new AdminReviewListDto
            {
                Id = r.Id,
                UserId = r.UserId,
                Username = r.User.Username,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                Rating = r.Rating,
                Comment = r.Comment,
                IsApproved = r.IsApproved,
                IsReported = r.IsReported,
                ModerationNote = r.ModerationNote,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(new PaginatedResult<AdminReviewListDto>
        {
            Items = reviews,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPut("reviews/{id}")]
    public async Task<IActionResult> ModerateReview(int id, [FromBody] ModerateReviewDto dto)
    {
        var review = await _context.ProductReviews.FindAsync(id);
        if (review == null) return NotFound(new { message = "Review not found." });

        review.IsApproved = dto.IsApproved;
        review.ModerationNote = dto.ModerationNote;
        review.IsReported = false;
        review.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Review moderated." });
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpDelete("reviews/{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _context.ProductReviews.FindAsync(id);
        if (review == null) return NotFound(new { message = "Review not found." });

        _context.ProductReviews.Remove(review);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Review deleted." });
    }

    [HttpGet("reviews/stats")]
    public async Task<IActionResult> GetReviewStats()
    {
        var reviews = await _context.ProductReviews.Include(r => r.Product).ToListAsync();
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var stats = new ReviewStatsDto
        {
            TotalReviews = reviews.Count,
            ApprovedReviews = reviews.Count(r => r.IsApproved),
            PendingReviews = reviews.Count(r => !r.IsApproved && !r.IsReported),
            ReportedReviews = reviews.Count(r => r.IsReported),
            AverageRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0,
            RatingDistribution = Enumerable.Range(1, 5).ToDictionary(i => i, i => reviews.Count(r => r.Rating == i)),
            MostReviewedProducts = reviews
                .GroupBy(r => r.ProductId)
                .Select(g => new MostReviewedProductDto
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product.Name,
                    ReviewCount = g.Count(),
                    AverageRating = g.Average(r => r.Rating)
                })
                .OrderByDescending(x => x.ReviewCount)
                .Take(10)
                .ToList(),
            ReviewsPerDay = reviews
                .Where(r => r.CreatedAt >= thirtyDaysAgo)
                .GroupBy(r => r.CreatedAt.Date)
                .Select(g => new DailyReviewDto { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToList()
        };

        return Ok(stats);
    }

    [HttpGet("comparisons/stats")]
    public async Task<IActionResult> GetComparisonStats()
    {
        var comparisons = await _context.ProductComparisons.ToListAsync();
        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-7);

        var allProductIds = comparisons
            .SelectMany(c => c.ProductIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Where(id => int.TryParse(id, out _))
            .Select(int.Parse)
            .ToList();

        var productNames = await _context.Products
            .Where(p => allProductIds.Distinct().Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name);

        var stats = new ComparisonStatsDto
        {
            TotalComparisons = comparisons.Count,
            ComparisonsToday = comparisons.Count(c => c.CreatedAt.Date == today),
            ComparisonsThisWeek = comparisons.Count(c => c.CreatedAt >= weekAgo),
            AvgProductsPerComparison = comparisons.Count > 0
                ? comparisons.Average(c => c.ProductIds.Split(',').Length)
                : 0,
            MostComparedProducts = allProductIds
                .GroupBy(id => id)
                .Select(g => new MostComparedProductDto
                {
                    ProductId = g.Key,
                    ProductName = productNames.GetValueOrDefault(g.Key, "Unknown"),
                    CompareCount = g.Count()
                })
                .OrderByDescending(x => x.CompareCount)
                .Take(10)
                .ToList()
        };

        return Ok(stats);
    }

    [HttpGet("shopping-lists/stats")]
    public async Task<IActionResult> GetShoppingListStats()
    {
        var lists = await _context.ShoppingLists.Include(sl => sl.Items).ToListAsync();

        var stats = new ShoppingListStatsDto
        {
            TotalLists = lists.Count,
            ActiveLists = lists.Count(sl => !sl.IsArchived),
            ArchivedLists = lists.Count(sl => sl.IsArchived),
            TotalItems = lists.Sum(sl => sl.Items.Count),
            CheckedItems = lists.Sum(sl => sl.Items.Count(i => i.IsChecked)),
            AvgItemsPerList = lists.Count > 0 ? lists.Average(sl => sl.Items.Count) : 0
        };

        return Ok(stats);
    }

    [HttpGet("allergen-alerts/stats")]
    public async Task<IActionResult> GetAllergenAlertStats()
    {
        var alerts = await _context.AllergenAlerts.Include(a => a.Product).ToListAsync();
        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-7);

        var stats = new AllergenAlertStatsDto
        {
            TotalAlerts = alerts.Count,
            AlertsToday = alerts.Count(a => a.CreatedAt.Date == today),
            AlertsThisWeek = alerts.Count(a => a.CreatedAt >= weekAgo),
            AllergenDistribution = alerts
                .SelectMany(a => a.MatchedAllergens.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .GroupBy(a => a)
                .ToDictionary(g => g.Key, g => g.Count()),
            MostAlertedProducts = alerts
                .GroupBy(a => a.ProductId)
                .Select(g => new MostAlertedProductDto
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product.Name,
                    AlertCount = g.Count()
                })
                .OrderByDescending(x => x.AlertCount)
                .Take(10)
                .ToList()
        };

        return Ok(stats);
    }

    // ==================== Sessions ====================

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int? userId = null,
        [FromQuery] string? deviceId = null)
    {
        var result = await _analyticsService.GetSessions(page, pageSize, from, to, userId, deviceId);
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("sessions/{id}/end")]
    public async Task<IActionResult> ForceEndSession(int id)
    {
        var session = await _context.AppSessions.FindAsync(id);
        if (session == null) return NotFound(new { message = "Session not found." });
        if (session.EndedAt != null) return BadRequest(new { message = "Session already ended." });

        session.EndedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _adminHub.Clients.Group("Admins").SendAsync("SessionEnded", new
        {
            sessionId = id,
            timestamp = DateTime.UtcNow
        });

        return Ok(new { message = "Session ended." });
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("sessions/end-all")]
    public async Task<IActionResult> ForceEndAllSessions([FromBody] EndSessionsDto? dto = null)
    {
        var query = _context.AppSessions.Where(s => s.EndedAt == null);

        if (dto?.UserId != null)
            query = query.Where(s => s.UserId == dto.UserId);
        if (!string.IsNullOrWhiteSpace(dto?.DeviceId))
            query = query.Where(s => s.DeviceId == dto.DeviceId);

        var sessions = await query.ToListAsync();
        foreach (var session in sessions)
            session.EndedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = $"{sessions.Count} session(s) ended.", count = sessions.Count });
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("users/{id}/force-logout")]
    public async Task<IActionResult> ForceLogoutUser(int id)
    {
        var user = await _context.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound(new { message = "User not found." });

        // Revoke all refresh tokens
        foreach (var token in user.RefreshTokens.Where(t => t.RevokedAt == null))
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokeReason = "Force logout by admin";
        }

        // End all active sessions
        var activeSessions = await _context.AppSessions
            .Where(s => s.UserId == id && s.EndedAt == null)
            .ToListAsync();
        foreach (var session in activeSessions)
            session.EndedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "User logged out from all devices.", sessionsEnded = activeSessions.Count });
    }

    // ==================== App Config ====================

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var config = await _context.AppConfigs.FirstOrDefaultAsync();
        if (config == null) return NotFound(new { message = "Config not found." });

        return Ok(new AppConfigDto
        {
            MaintenanceMode = config.MaintenanceMode,
            MaintenanceMessage = config.MaintenanceMessage,
            MinAppVersion = config.MinAppVersion,
            LatestAppVersion = config.LatestAppVersion,
            ForceUpdateMessage = config.ForceUpdateMessage,
            SessionTimeoutMinutes = config.SessionTimeoutMinutes,
            ScanEnabled = config.ScanEnabled,
            SearchEnabled = config.SearchEnabled,
            FavoritesEnabled = config.FavoritesEnabled,
            RegistrationEnabled = config.RegistrationEnabled,
            HistoryEnabled = config.HistoryEnabled,
            OpenFoodFactsEnabled = config.OpenFoodFactsEnabled,
            BiometricAuthEnabled = config.BiometricAuthEnabled,
            AccountDeletionEnabled = config.AccountDeletionEnabled,
            PasswordResetEnabled = config.PasswordResetEnabled,
            UpdatedAt = config.UpdatedAt
        });
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateAppConfigDto dto)
    {
        var config = await _context.AppConfigs.FirstOrDefaultAsync();
        if (config == null)
        {
            config = new AppConfig();
            _context.AppConfigs.Add(config);
        }

        config.MaintenanceMode = dto.MaintenanceMode;
        config.MaintenanceMessage = dto.MaintenanceMessage;
        config.MinAppVersion = dto.MinAppVersion;
        config.LatestAppVersion = dto.LatestAppVersion;
        config.ForceUpdateMessage = dto.ForceUpdateMessage;
        config.SessionTimeoutMinutes = dto.SessionTimeoutMinutes;
        config.ScanEnabled = dto.ScanEnabled;
        config.SearchEnabled = dto.SearchEnabled;
        config.FavoritesEnabled = dto.FavoritesEnabled;
        config.RegistrationEnabled = dto.RegistrationEnabled;
        config.HistoryEnabled = dto.HistoryEnabled;
        config.OpenFoodFactsEnabled = dto.OpenFoodFactsEnabled;
        config.BiometricAuthEnabled = dto.BiometricAuthEnabled;
        config.AccountDeletionEnabled = dto.AccountDeletionEnabled;
        config.PasswordResetEnabled = dto.PasswordResetEnabled;
        config.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Settings updated." });
    }

    // ==================== Analytics ====================

    [HttpGet("analytics/events")]
    public async Task<IActionResult> GetEvents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int? userId = null,
        [FromQuery] string? eventType = null)
    {
        var result = await _analyticsService.GetEvents(page, pageSize, from, to, userId, eventType);
        return Ok(result);
    }

    [HttpGet("analytics/pageviews")]
    public async Task<IActionResult> GetPageViews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int? userId = null,
        [FromQuery] string? pageName = null)
    {
        var result = await _analyticsService.GetPageViews(page, pageSize, from, to, userId, pageName);
        return Ok(result);
    }

    [HttpGet("analytics/devices")]
    public async Task<IActionResult> GetDeviceStats()
    {
        var stats = await _analyticsService.GetDeviceStats();
        return Ok(stats);
    }

    [HttpGet("analytics/retention")]
    public async Task<IActionResult> GetRetention()
    {
        var data = await _analyticsService.GetRetentionData();
        return Ok(data);
    }

    [HttpGet("analytics/realtime")]
    public async Task<IActionResult> GetRealTime()
    {
        var stats = await _analyticsService.GetRealTimeStats();
        return Ok(stats);
    }

    [HttpGet("analytics/searches")]
    public async Task<IActionResult> GetTopSearches([FromQuery] int days = 30)
    {
        var data = await _analyticsService.GetTopSearchQueries(days);
        return Ok(data);
    }

    [HttpGet("analytics/api-logs")]
    public async Task<IActionResult> GetApiLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? method = null,
        [FromQuery] string? path = null,
        [FromQuery] int? statusCode = null)
    {
        var result = await _analyticsService.GetApiLogs(page, pageSize, from, to, method, path, statusCode);
        return Ok(result);
    }

    [HttpGet("analytics/summary")]
    public async Task<IActionResult> GetAnalyticsSummary()
    {
        var data = await _analyticsService.GetAnalyticsSummary();
        return Ok(data);
    }

    [HttpGet("analytics/trends")]
    public async Task<IActionResult> GetEventTrends([FromQuery] int days = 30)
    {
        var data = await _analyticsService.GetEventTrends(days);
        return Ok(data);
    }

    [HttpGet("analytics/user-growth")]
    public async Task<IActionResult> GetUserGrowth([FromQuery] int days = 30)
    {
        var data = await _analyticsService.GetUserGrowth(days);
        return Ok(data);
    }

    [HttpGet("analytics/peak-hours")]
    public async Task<IActionResult> GetPeakHours()
    {
        var data = await _analyticsService.GetPeakHours();
        return Ok(data);
    }

    [HttpGet("analytics/funnel")]
    public async Task<IActionResult> GetFunnel()
    {
        var data = await _analyticsService.GetFunnelStats();
        return Ok(data);
    }
}
