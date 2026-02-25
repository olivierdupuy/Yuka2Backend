using Microsoft.EntityFrameworkCore;
using Yuka2Back.Data;
using Yuka2Back.DTOs;
using Yuka2Back.Models;

namespace Yuka2Back.Services;

public class AnalyticsService
{
    private readonly AppDbContext _context;

    public AnalyticsService(AppDbContext context)
    {
        _context = context;
    }

    // ==================== Tracking Methods ====================

    public async Task<AppSession> StartSession(string deviceId, string? deviceModel, string? deviceOS,
        string? appVersion, int? userId, string? ipAddress)
    {
        var session = new AppSession
        {
            UserId = userId,
            DeviceId = deviceId,
            DeviceModel = deviceModel,
            DeviceOS = deviceOS,
            AppVersion = appVersion,
            StartedAt = DateTime.UtcNow,
            IpAddress = ipAddress
        };

        _context.AppSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<bool> EndSession(int sessionId)
    {
        var session = await _context.AppSessions.FindAsync(sessionId);
        if (session == null) return false;

        session.EndedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PageView> TrackPageView(int sessionId, string pageName, int? userId)
    {
        // Close previous page view in the same session that hasn't been closed
        var openPageView = await _context.PageViews
            .Where(pv => pv.SessionId == sessionId && pv.ExitedAt == null)
            .OrderByDescending(pv => pv.EnteredAt)
            .FirstOrDefaultAsync();

        if (openPageView != null)
        {
            openPageView.ExitedAt = DateTime.UtcNow;
        }

        var pageView = new PageView
        {
            SessionId = sessionId,
            UserId = userId,
            PageName = pageName,
            EnteredAt = DateTime.UtcNow
        };

        _context.PageViews.Add(pageView);
        await _context.SaveChangesAsync();
        return pageView;
    }

    public async Task<AnalyticsEvent> TrackEvent(string eventType, string? eventData,
        int? userId, int? sessionId, string? deviceId, string? ipAddress)
    {
        var analyticsEvent = new AnalyticsEvent
        {
            EventType = eventType,
            EventData = eventData,
            UserId = userId,
            SessionId = sessionId,
            DeviceId = deviceId,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        _context.AnalyticsEvents.Add(analyticsEvent);
        await _context.SaveChangesAsync();
        return analyticsEvent;
    }

    public async Task LogApiCall(string method, string path, int statusCode,
        long durationMs, int? userId, string? ipAddress, string? userAgent)
    {
        var log = new ApiLog
        {
            Method = method,
            Path = path,
            StatusCode = statusCode,
            DurationMs = durationMs,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent?.Length > 500 ? userAgent[..500] : userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApiLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    // ==================== Dashboard / Stats ====================

    public async Task<DashboardStatsDto> GetDashboardStats()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = now.AddDays(-7);
        var monthAgo = now.AddDays(-30);

        var totalUsers = await _context.Users.CountAsync();
        var totalScans = await _context.ScanHistories.CountAsync();
        var totalProducts = await _context.Products.CountAsync();

        // Active users based on events
        var activeToday = await _context.AnalyticsEvents
            .Where(e => e.CreatedAt >= today && e.UserId != null)
            .Select(e => e.UserId)
            .Distinct()
            .CountAsync();

        var activeWeek = await _context.AnalyticsEvents
            .Where(e => e.CreatedAt >= weekAgo && e.UserId != null)
            .Select(e => e.UserId)
            .Distinct()
            .CountAsync();

        var activeMonth = await _context.AnalyticsEvents
            .Where(e => e.CreatedAt >= monthAgo && e.UserId != null)
            .Select(e => e.UserId)
            .Distinct()
            .CountAsync();

        // Avg session duration (only completed sessions)
        var avgDuration = await _context.AppSessions
            .Where(s => s.EndedAt != null)
            .AverageAsync(s => (double?)EF.Functions.DateDiffSecond(s.StartedAt, s.EndedAt!.Value)) ?? 0;

        // Most scanned products (top 10)
        var mostScanned = await _context.ScanHistories
            .GroupBy(s => s.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .Join(_context.Products,
                s => s.ProductId,
                p => p.Id,
                (s, p) => new MostScannedProductDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Brand = p.Brand,
                    ScanCount = s.Count
                })
            .ToListAsync();

        // Most active users (top 10 by events in last 30 days)
        var mostActive = await _context.AnalyticsEvents
            .Where(e => e.CreatedAt >= monthAgo && e.UserId != null)
            .GroupBy(e => e.UserId!.Value)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .Join(_context.Users,
                e => e.UserId,
                u => u.Id,
                (e, u) => new MostActiveUserDto
                {
                    UserId = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    EventCount = e.Count
                })
            .ToListAsync();

        // Hourly activity (last 24 hours)
        var dayAgo = now.AddHours(-24);
        var hourlyEvents = await _context.AnalyticsEvents
            .Where(e => e.CreatedAt >= dayAgo)
            .GroupBy(e => e.CreatedAt.Hour)
            .Select(g => new HourlyActivityDto
            {
                Hour = g.Key,
                EventCount = g.Count()
            })
            .OrderBy(h => h.Hour)
            .ToListAsync();

        return new DashboardStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsersToday = activeToday,
            ActiveUsersThisWeek = activeWeek,
            ActiveUsersThisMonth = activeMonth,
            TotalScans = totalScans,
            TotalProducts = totalProducts,
            AvgSessionDurationSeconds = avgDuration,
            MostScannedProducts = mostScanned,
            MostActiveUsers = mostActive,
            HourlyActivity = hourlyEvents
        };
    }

    public async Task<OverviewStatsDto> GetOverviewStats()
    {
        var today = DateTime.UtcNow.Date;

        return new OverviewStatsDto
        {
            TotalUsers = await _context.Users.CountAsync(),
            TotalProducts = await _context.Products.CountAsync(),
            TotalScans = await _context.ScanHistories.CountAsync(),
            TotalSessions = await _context.AppSessions.CountAsync(),
            TotalEvents = await _context.AnalyticsEvents.CountAsync(),
            ActiveSessionsNow = await _context.AppSessions.CountAsync(s => s.EndedAt == null),
            NewUsersToday = await _context.Users.CountAsync(u => u.CreatedAt >= today),
            ScansToday = await _context.ScanHistories.CountAsync(s => s.ScannedAt >= today)
        };
    }

    // ==================== Paginated Queries ====================

    public async Task<PaginatedResult<SessionListDto>> GetSessions(
        int page, int pageSize, DateTime? from, DateTime? to, int? userId, string? deviceId)
    {
        var query = _context.AppSessions.AsQueryable();

        if (from.HasValue) query = query.Where(s => s.StartedAt >= from.Value);
        if (to.HasValue) query = query.Where(s => s.StartedAt <= to.Value);
        if (userId.HasValue) query = query.Where(s => s.UserId == userId.Value);
        if (!string.IsNullOrWhiteSpace(deviceId)) query = query.Where(s => s.DeviceId == deviceId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SessionListDto
            {
                Id = s.Id,
                UserId = s.UserId,
                DeviceId = s.DeviceId,
                DeviceModel = s.DeviceModel,
                DeviceOS = s.DeviceOS,
                AppVersion = s.AppVersion,
                StartedAt = s.StartedAt,
                EndedAt = s.EndedAt,
                IsActive = s.EndedAt == null,
                IpAddress = s.IpAddress,
                Country = s.Country,
                City = s.City,
                PageViewCount = s.PageViews.Count,
                EventCount = s.Events.Count
            })
            .ToListAsync();

        // Fill in usernames
        var userIds = items.Where(i => i.UserId.HasValue).Select(i => i.UserId!.Value).Distinct().ToList();
        if (userIds.Count > 0)
        {
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            foreach (var item in items)
            {
                if (item.UserId.HasValue && users.TryGetValue(item.UserId.Value, out var username))
                    item.Username = username;
            }
        }

        return new PaginatedResult<SessionListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResult<EventListDto>> GetEvents(
        int page, int pageSize, DateTime? from, DateTime? to, int? userId, string? eventType)
    {
        var query = _context.AnalyticsEvents.AsQueryable();

        if (from.HasValue) query = query.Where(e => e.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(e => e.CreatedAt <= to.Value);
        if (userId.HasValue) query = query.Where(e => e.UserId == userId.Value);
        if (!string.IsNullOrWhiteSpace(eventType)) query = query.Where(e => e.EventType == eventType);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EventListDto
            {
                Id = e.Id,
                SessionId = e.SessionId,
                UserId = e.UserId,
                EventType = e.EventType,
                EventData = e.EventData,
                CreatedAt = e.CreatedAt,
                IpAddress = e.IpAddress,
                DeviceId = e.DeviceId
            })
            .ToListAsync();

        // Fill in usernames
        var userIds = items.Where(i => i.UserId.HasValue).Select(i => i.UserId!.Value).Distinct().ToList();
        if (userIds.Count > 0)
        {
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            foreach (var item in items)
            {
                if (item.UserId.HasValue && users.TryGetValue(item.UserId.Value, out var username))
                    item.Username = username;
            }
        }

        return new PaginatedResult<EventListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResult<PageViewListDto>> GetPageViews(
        int page, int pageSize, DateTime? from, DateTime? to, int? userId, string? pageName)
    {
        var query = _context.PageViews.AsQueryable();

        if (from.HasValue) query = query.Where(pv => pv.EnteredAt >= from.Value);
        if (to.HasValue) query = query.Where(pv => pv.EnteredAt <= to.Value);
        if (userId.HasValue) query = query.Where(pv => pv.UserId == userId.Value);
        if (!string.IsNullOrWhiteSpace(pageName)) query = query.Where(pv => pv.PageName.Contains(pageName));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(pv => pv.EnteredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(pv => new PageViewListDto
            {
                Id = pv.Id,
                SessionId = pv.SessionId,
                UserId = pv.UserId,
                PageName = pv.PageName,
                EnteredAt = pv.EnteredAt,
                ExitedAt = pv.ExitedAt,
                DurationSeconds = pv.ExitedAt.HasValue
                    ? EF.Functions.DateDiffSecond(pv.EnteredAt, pv.ExitedAt.Value)
                    : null
            })
            .ToListAsync();

        return new PaginatedResult<PageViewListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResult<ApiLogListDto>> GetApiLogs(
        int page, int pageSize, DateTime? from, DateTime? to, string? method, string? path, int? statusCode)
    {
        var query = _context.ApiLogs.AsQueryable();

        if (from.HasValue) query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(l => l.CreatedAt <= to.Value);
        if (!string.IsNullOrWhiteSpace(method)) query = query.Where(l => l.Method == method);
        if (!string.IsNullOrWhiteSpace(path)) query = query.Where(l => l.Path.Contains(path));
        if (statusCode.HasValue) query = query.Where(l => l.StatusCode == statusCode.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new ApiLogListDto
            {
                Id = l.Id,
                Method = l.Method,
                Path = l.Path,
                StatusCode = l.StatusCode,
                DurationMs = l.DurationMs,
                UserId = l.UserId,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResult<ApiLogListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ==================== User Analytics ====================

    public async Task<AdminUserDetailDto?> GetUserAnalytics(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        var totalFavorites = await _context.FavoriteProducts.CountAsync(f => f.UserId == userId);
        var totalSessions = await _context.AppSessions.CountAsync(s => s.UserId == userId);
        var totalEvents = await _context.AnalyticsEvents.CountAsync(e => e.UserId == userId);

        var recentActivity = await _context.AnalyticsEvents
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(20)
            .Select(e => new RecentActivityDto
            {
                EventType = e.EventType,
                EventData = e.EventData,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return new AdminUserDetailDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            TotalScans = user.TotalScans,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            IsActive = user.LockoutEnd == null || user.LockoutEnd < DateTime.UtcNow,
            DietType = user.DietType,
            Allergies = user.Allergies,
            TotalFavorites = totalFavorites,
            TotalSessions = totalSessions,
            TotalEvents = totalEvents,
            RecentActivity = recentActivity
        };
    }

    // ==================== Product Analytics ====================

    public async Task<ProductAnalyticsDto?> GetProductAnalytics(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) return null;

        var totalScans = await _context.ScanHistories.CountAsync(s => s.ProductId == productId);
        var totalFavorites = await _context.FavoriteProducts.CountAsync(f => f.ProductId == productId);
        var uniqueUsers = await _context.ScanHistories
            .Where(s => s.ProductId == productId)
            .Select(s => s.UserId)
            .Distinct()
            .CountAsync();

        var totalViews = await _context.AnalyticsEvents
            .CountAsync(e => e.EventType == "product_view" &&
                             e.EventData != null && e.EventData.Contains(productId.ToString()));

        // Daily scans over last 30 days
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30).Date;
        var dailyScans = await _context.ScanHistories
            .Where(s => s.ProductId == productId && s.ScannedAt >= thirtyDaysAgo)
            .GroupBy(s => s.ScannedAt.Date)
            .Select(g => new DailyCountDto
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        return new ProductAnalyticsDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Brand = product.Brand,
            TotalScans = totalScans,
            TotalViews = totalViews,
            TotalFavorites = totalFavorites,
            UniqueUsers = uniqueUsers,
            DailyScans = dailyScans
        };
    }

    // ==================== Search Queries ====================

    public async Task<List<TopSearchQueryDto>> GetTopSearchQueries(int days)
    {
        var since = DateTime.UtcNow.AddDays(-days);

        return await _context.AnalyticsEvents
            .Where(e => e.EventType == "search" && e.EventData != null && e.CreatedAt >= since)
            .GroupBy(e => e.EventData!)
            .Select(g => new TopSearchQueryDto
            {
                Query = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(q => q.Count)
            .Take(50)
            .ToListAsync();
    }

    // ==================== Device Stats ====================

    public async Task<DeviceStatsDto> GetDeviceStats()
    {
        var totalSessions = await _context.AppSessions.CountAsync();
        if (totalSessions == 0) totalSessions = 1; // avoid division by zero

        var osDist = await _context.AppSessions
            .Where(s => s.DeviceOS != null)
            .GroupBy(s => s.DeviceOS!)
            .Select(g => new DeviceDistributionDto
            {
                Name = g.Key,
                Count = g.Count(),
                Percentage = Math.Round(g.Count() * 100.0 / totalSessions, 1)
            })
            .OrderByDescending(d => d.Count)
            .Take(20)
            .ToListAsync();

        var deviceModels = await _context.AppSessions
            .Where(s => s.DeviceModel != null)
            .GroupBy(s => s.DeviceModel!)
            .Select(g => new DeviceDistributionDto
            {
                Name = g.Key,
                Count = g.Count(),
                Percentage = Math.Round(g.Count() * 100.0 / totalSessions, 1)
            })
            .OrderByDescending(d => d.Count)
            .Take(20)
            .ToListAsync();

        var appVersions = await _context.AppSessions
            .Where(s => s.AppVersion != null)
            .GroupBy(s => s.AppVersion!)
            .Select(g => new DeviceDistributionDto
            {
                Name = g.Key,
                Count = g.Count(),
                Percentage = Math.Round(g.Count() * 100.0 / totalSessions, 1)
            })
            .OrderByDescending(d => d.Count)
            .Take(20)
            .ToListAsync();

        return new DeviceStatsDto
        {
            OSDistribution = osDist,
            DeviceModels = deviceModels,
            AppVersions = appVersions
        };
    }

    // ==================== Retention ====================

    public async Task<RetentionDataDto> GetRetentionData()
    {
        var sixtyDaysAgo = DateTime.UtcNow.AddDays(-60).Date;

        // Daily active users (last 60 days)
        var dailyActive = await _context.AnalyticsEvents
            .Where(e => e.CreatedAt >= sixtyDaysAgo && e.UserId != null)
            .GroupBy(e => e.CreatedAt.Date)
            .Select(g => new DailyCountDto
            {
                Date = g.Key,
                Count = g.Select(e => e.UserId).Distinct().Count()
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        // Weekly active users (last 12 weeks)
        var twelveWeeksAgo = DateTime.UtcNow.AddDays(-84).Date;
        var weeklyEvents = await _context.AnalyticsEvents
            .Where(e => e.CreatedAt >= twelveWeeksAgo && e.UserId != null)
            .Select(e => new { e.UserId, e.CreatedAt })
            .ToListAsync();

        var weeklyActive = weeklyEvents
            .GroupBy(e => new
            {
                Year = System.Globalization.ISOWeek.GetYear(e.CreatedAt),
                Week = System.Globalization.ISOWeek.GetWeekOfYear(e.CreatedAt)
            })
            .Select(g => new WeeklyCountDto
            {
                Year = g.Key.Year,
                Week = g.Key.Week,
                Count = g.Select(e => e.UserId).Distinct().Count()
            })
            .OrderBy(w => w.Year).ThenBy(w => w.Week)
            .ToList();

        // Monthly active users (last 12 months)
        var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12).Date;
        var monthlyEvents = await _context.AnalyticsEvents
            .Where(e => e.CreatedAt >= twelveMonthsAgo && e.UserId != null)
            .Select(e => new { e.UserId, e.CreatedAt })
            .ToListAsync();

        var monthlyActive = monthlyEvents
            .GroupBy(e => new { e.CreatedAt.Year, e.CreatedAt.Month })
            .Select(g => new MonthlyCountDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Select(e => e.UserId).Distinct().Count()
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        return new RetentionDataDto
        {
            DailyActiveUsers = dailyActive,
            WeeklyActiveUsers = weeklyActive,
            MonthlyActiveUsers = monthlyActive
        };
    }

    // ==================== Real-time ====================

    public async Task<RealTimeStatsDto> GetRealTimeStats()
    {
        var fiveMinAgo = DateTime.UtcNow.AddMinutes(-5);

        var activeSessionsNow = await _context.AppSessions
            .CountAsync(s => s.EndedAt == null);

        var activeUsersNow = await _context.AppSessions
            .Where(s => s.EndedAt == null && s.UserId != null)
            .Select(s => s.UserId)
            .Distinct()
            .CountAsync();

        var pageViewsLast5 = await _context.PageViews
            .CountAsync(pv => pv.EnteredAt >= fiveMinAgo);

        var recentEventsQuery = _context.AnalyticsEvents
            .Where(e => e.CreatedAt >= fiveMinAgo);

        var eventsLast5 = await recentEventsQuery.CountAsync();

        var scansLast5 = await recentEventsQuery
            .CountAsync(e => e.EventType == "scan");

        var recentPages = await _context.PageViews
            .Where(pv => pv.EnteredAt >= fiveMinAgo)
            .OrderByDescending(pv => pv.EnteredAt)
            .Select(pv => pv.PageName)
            .Take(20)
            .ToListAsync();

        var recentEvents = await _context.AnalyticsEvents
            .Where(e => e.CreatedAt >= fiveMinAgo)
            .OrderByDescending(e => e.CreatedAt)
            .Take(15)
            .Select(e => new RealtimeEventDto
            {
                EventType = e.EventType,
                EventData = e.EventData,
                Username = e.UserId != null
                    ? _context.Users.Where(u => u.Id == e.UserId).Select(u => u.Username).FirstOrDefault()
                    : null,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        var eventTypeCounts = await _context.AnalyticsEvents
            .Where(e => e.CreatedAt >= fiveMinAgo)
            .GroupBy(e => e.EventType)
            .Select(g => new EventTypeCountDto
            {
                EventType = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return new RealTimeStatsDto
        {
            ActiveSessionsNow = activeSessionsNow,
            ActiveUsersNow = activeUsersNow,
            PageViewsLast5Min = pageViewsLast5,
            EventsLast5Min = eventsLast5,
            ScansLast5Min = scansLast5,
            RecentPages = recentPages,
            RecentEvents = recentEvents,
            EventTypeCounts = eventTypeCounts
        };
    }

    // ==================== Admin User Management Helpers ====================

    public async Task<PaginatedResult<AdminUserListDto>> GetUsers(
        int page, int pageSize, string? search, string? sortBy, bool sortDesc)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                u.Username.Contains(search) ||
                u.Email.Contains(search) ||
                (u.FirstName != null && u.FirstName.Contains(search)) ||
                (u.LastName != null && u.LastName.Contains(search)));
        }

        var totalCount = await query.CountAsync();

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "username" => sortDesc ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
            "email" => sortDesc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "totalscans" => sortDesc ? query.OrderByDescending(u => u.TotalScans) : query.OrderBy(u => u.TotalScans),
            "lastloginat" => sortDesc ? query.OrderByDescending(u => u.LastLoginAt) : query.OrderBy(u => u.LastLoginAt),
            "createdat" => sortDesc ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserListDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                TotalScans = u.TotalScans,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt,
                IsActive = u.LockoutEnd == null || u.LockoutEnd < DateTime.UtcNow
            })
            .ToListAsync();

        return new PaginatedResult<AdminUserListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResult<AdminProductListDto>> GetProducts(
        int page, int pageSize, string? search)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Name.Contains(search) ||
                p.Barcode.Contains(search) ||
                (p.Brand != null && p.Brand.Contains(search)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new AdminProductListDto
            {
                Id = p.Id,
                Barcode = p.Barcode,
                Name = p.Name,
                Brand = p.Brand,
                NutriScore = p.NutriScore,
                HealthScore = p.HealthScore,
                ScanCount = p.ScanHistories.Count,
                FavoriteCount = p.FavoriteProducts.Count,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResult<AdminProductListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
