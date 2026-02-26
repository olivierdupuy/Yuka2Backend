using Microsoft.EntityFrameworkCore;
using Yuka2Back.Data;

namespace Yuka2Back.Services;

/// <summary>
/// Background service that periodically closes stale sessions based on configurable timeout.
/// </summary>
public class SessionCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SessionCleanupService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(10);

    public SessionCleanupService(IServiceScopeFactory scopeFactory, ILogger<SessionCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupStaleSessions(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session cleanup");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CleanupStaleSessions(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Read timeout from AppConfig (default 30 min if not configured)
        var config = await context.AppConfigs.FirstOrDefaultAsync(ct);
        var timeoutMinutes = config?.SessionTimeoutMinutes ?? 30;
        var cutoff = DateTime.UtcNow.AddMinutes(-timeoutMinutes);

        // Find active sessions whose last activity is older than the timeout.
        var staleSessions = await context.AppSessions
            .Where(s => s.EndedAt == null)
            .Where(s =>
                // No activity at all and started before cutoff
                (!s.PageViews.Any() && !s.Events.Any() && s.StartedAt < cutoff)
                ||
                // Has activity but the most recent one is before cutoff
                (s.PageViews.Any() && s.PageViews.Max(pv => pv.EnteredAt) < cutoff
                    && (!s.Events.Any() || s.Events.Max(e => e.CreatedAt) < cutoff))
                ||
                (s.Events.Any() && s.Events.Max(e => e.CreatedAt) < cutoff
                    && (!s.PageViews.Any() || s.PageViews.Max(pv => pv.EnteredAt) < cutoff))
            )
            .ToListAsync(ct);

        if (staleSessions.Count > 0)
        {
            foreach (var session in staleSessions)
            {
                session.EndedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync(ct);
            _logger.LogInformation("Closed {Count} stale sessions (timeout: {Timeout}min)", staleSessions.Count, timeoutMinutes);
        }
    }
}
