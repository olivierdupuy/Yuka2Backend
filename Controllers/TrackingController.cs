using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Yuka2Back.DTOs;
using Yuka2Back.Hubs;
using Yuka2Back.Services;

namespace Yuka2Back.Controllers;

[ApiController]
[Route("api/tracking")]
public class TrackingController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;
    private readonly IConfiguration _config;
    private readonly IHubContext<AdminHub> _adminHub;

    public TrackingController(AnalyticsService analyticsService, IConfiguration config, IHubContext<AdminHub> adminHub)
    {
        _analyticsService = analyticsService;
        _config = config;
        _adminHub = adminHub;
    }

    /// <summary>
    /// Extracts the userId from Authorization header if present, without requiring auth.
    /// </summary>
    private int? TryGetUserId()
    {
        try
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            var token = authHeader["Bearer ".Length..].Trim();
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : null;
        }
        catch
        {
            return null;
        }
    }

    private string? GetIpAddress() =>
        HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpPost("session/start")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionDto dto)
    {
        var userId = TryGetUserId();
        var session = await _analyticsService.StartSession(
            dto.DeviceId, dto.DeviceModel, dto.DeviceOS,
            dto.AppVersion, userId, GetIpAddress(), dto.PreviousSessionId);

        await _adminHub.Clients.Group("Admins").SendAsync("SessionStarted", new
        {
            sessionId = session.Id,
            deviceModel = dto.DeviceModel,
            deviceOS = dto.DeviceOS,
            timestamp = DateTime.UtcNow
        });

        return Ok(new { sessionId = session.Id });
    }

    [HttpPost("session/end")]
    public async Task<IActionResult> EndSession([FromBody] EndSessionDto dto)
    {
        var success = await _analyticsService.EndSession(dto.SessionId);
        if (!success)
            return NotFound(new { message = "Session not found." });

        await _adminHub.Clients.Group("Admins").SendAsync("SessionEnded", new
        {
            sessionId = dto.SessionId,
            timestamp = DateTime.UtcNow
        });

        return Ok(new { message = "Session ended." });
    }

    [HttpPost("pageview")]
    public async Task<IActionResult> TrackPageView([FromBody] TrackPageViewDto dto)
    {
        var userId = TryGetUserId();
        var pageView = await _analyticsService.TrackPageView(dto.SessionId, dto.PageName, userId);

        await _adminHub.Clients.Group("Admins").SendAsync("PageViewed", new
        {
            pageName = dto.PageName,
            timestamp = DateTime.UtcNow
        });

        return Ok(new { pageViewId = pageView.Id });
    }

    [HttpPost("event")]
    public async Task<IActionResult> TrackEvent([FromBody] TrackEventDto dto)
    {
        var userId = TryGetUserId();
        var analyticsEvent = await _analyticsService.TrackEvent(
            dto.EventType, dto.EventData,
            userId, dto.SessionId, dto.DeviceId, GetIpAddress());

        await _adminHub.Clients.Group("Admins").SendAsync("NewEvent", new
        {
            eventType = dto.EventType,
            eventData = dto.EventData,
            timestamp = DateTime.UtcNow
        });

        return Ok(new { eventId = analyticsEvent.Id });
    }
}
