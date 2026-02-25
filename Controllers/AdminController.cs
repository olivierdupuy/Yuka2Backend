using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yuka2Back.Data;
using Yuka2Back.DTOs;
using Yuka2Back.Services;

namespace Yuka2Back.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "SuperAdmin,Admin,Viewer")]
public class AdminController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;
    private readonly AppDbContext _context;

    public AdminController(AnalyticsService analyticsService, AppDbContext context)
    {
        _analyticsService = analyticsService;
        _context = context;
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
        [FromQuery] bool sortDesc = true)
    {
        var result = await _analyticsService.GetUsers(page, pageSize, search, sortBy, sortDesc);
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
}
