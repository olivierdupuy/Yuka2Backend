using System.Diagnostics;
using System.Security.Claims;
using Yuka2Back.Services;

namespace Yuka2Back.Middleware;

public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public ApiLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip tracking endpoints to avoid recursion
        if (path.StartsWith("/api/tracking", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        // Log the API call asynchronously (fire-and-forget with a new scope)
        var method = context.Request.Method;
        var statusCode = context.Response.StatusCode;
        var durationMs = stopwatch.ElapsedMilliseconds;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers.UserAgent.FirstOrDefault();

        int? userId = null;
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && int.TryParse(userIdClaim, out var parsedId))
        {
            userId = parsedId;
        }

        // Use a separate scope so we don't interfere with the request's DbContext
        var scopeFactory = context.RequestServices.GetRequiredService<IServiceScopeFactory>();
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var analyticsService = scope.ServiceProvider.GetRequiredService<AnalyticsService>();
                await analyticsService.LogApiCall(method, path, statusCode, durationMs, userId, ipAddress, userAgent);
            }
            catch
            {
                // Logging failures should not crash the application
            }
        });
    }
}
