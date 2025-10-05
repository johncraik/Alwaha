using AlwahaLibrary.Data;
using Microsoft.EntityFrameworkCore;

namespace AlwahaManagement.Services;

public class AnalyticsService
{
    private readonly AlwahaDbContext _context;

    public AnalyticsService(AlwahaDbContext context)
    {
        _context = context;
    }

    public async Task<AnalyticsSummary> GetSummaryAsync(DateTime startDate, DateTime endDate)
    {
        var events = await _context.AnalyticsEvents
            .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
            .ToListAsync();

        var cfData = await _context.CloudflareAnalytics
            .Where(c => c.Date >= startDate.Date && c.Date <= endDate.Date)
            .ToListAsync();

        // Use Cloudflare data if available, otherwise fall back to manual tracking
        var hasCloudflareData = cfData.Any();
        var totalPageViews = hasCloudflareData
            ? cfData.Sum(c => c.PageViews)
            : events.Count(e => e.EventType == "PageView");
        var uniqueVisitors = hasCloudflareData
            ? cfData.Sum(c => c.UniqueVisitors)
            : events.Where(e => e.EventType == "PageView").Select(e => e.SessionId).Distinct().Count();

        return new AnalyticsSummary
        {
            TotalPageViews = totalPageViews,
            UniqueVisitors = (int)uniqueVisitors,
            TotalRequests = cfData.Sum(c => c.Requests),
            BandwidthUsed = cfData.Sum(c => c.BandwidthTotal),
            ThreatsBlocked = cfData.Sum(c => c.ThreatsBlocked),
            AverageCacheHitRate = cfData.Any() ? cfData.Average(c => c.CacheHitRate) : 0
        };
    }

    public async Task<List<PageViewData>> GetPageViewsOverTimeAsync(DateTime startDate, DateTime endDate, string interval = "day")
    {
        // Try Cloudflare data first for daily views
        var cfData = await _context.CloudflareAnalytics
            .Where(c => c.Date >= startDate.Date && c.Date <= endDate.Date)
            .OrderBy(c => c.Date)
            .ToListAsync();

        if (cfData.Any() && interval == "day")
        {
            // Fill in missing dates with 0 values
            var result = new List<PageViewData>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var cfEntry = cfData.FirstOrDefault(c => c.Date.Date == date);
                result.Add(new PageViewData
                {
                    Date = date,
                    Count = cfEntry != null ? (int)cfEntry.PageViews : 0
                });
            }
            return result;
        }

        // Fall back to manual tracking
        var events = await _context.AnalyticsEvents
            .Where(e => e.EventType == "PageView" && e.Timestamp >= startDate && e.Timestamp <= endDate)
            .ToListAsync();

        var grouped = interval == "hour"
            ? events.GroupBy(e => new { e.Timestamp.Year, e.Timestamp.Month, e.Timestamp.Day, e.Timestamp.Hour })
                    .Select(g => new PageViewData
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
                        Count = g.Count()
                    })
            : events.GroupBy(e => e.Timestamp.Date)
                    .Select(g => new PageViewData
                    {
                        Date = g.Key,
                        Count = g.Count()
                    });

        var groupedList = grouped.OrderBy(d => d.Date).ToList();

        // Fill in missing dates
        if (interval == "day")
        {
            var result = new List<PageViewData>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var existing = groupedList.FirstOrDefault(g => g.Date.Date == date);
                result.Add(existing ?? new PageViewData { Date = date, Count = 0 });
            }
            return result;
        }

        return groupedList;
    }

    public async Task<List<PageViewData>> GetUniqueVisitorsOverTimeAsync(DateTime startDate, DateTime endDate, string interval = "day")
    {
        // Try Cloudflare data first for daily unique visitors
        var cfData = await _context.CloudflareAnalytics
            .Where(c => c.Date >= startDate.Date && c.Date <= endDate.Date)
            .OrderBy(c => c.Date)
            .ToListAsync();

        if (cfData.Any() && interval == "day")
        {
            // Fill in missing dates with 0 values
            var result = new List<PageViewData>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var cfEntry = cfData.FirstOrDefault(c => c.Date.Date == date);
                result.Add(new PageViewData
                {
                    Date = date,
                    Count = cfEntry != null ? (int)cfEntry.UniqueVisitors : 0
                });
            }
            return result;
        }

        // Fall back to manual tracking - count unique sessions per day
        var events = await _context.AnalyticsEvents
            .Where(e => e.EventType == "PageView" && e.Timestamp >= startDate && e.Timestamp <= endDate)
            .ToListAsync();

        var grouped = events.GroupBy(e => e.Timestamp.Date)
                    .Select(g => new PageViewData
                    {
                        Date = g.Key,
                        Count = g.Select(e => e.SessionId).Distinct().Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

        // Fill in missing dates
        var finalResult = new List<PageViewData>();
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var existing = grouped.FirstOrDefault(g => g.Date.Date == date);
            finalResult.Add(existing ?? new PageViewData { Date = date, Count = 0 });
        }

        return finalResult;
    }

    public async Task<List<TopPageData>> GetTopPagesAsync(DateTime startDate, DateTime endDate, int limit = 10)
    {
        var topPages = await _context.AnalyticsEvents
            .Where(e => e.EventType == "PageView" && e.Timestamp >= startDate && e.Timestamp <= endDate)
            .GroupBy(e => new { e.Url, e.PageTitle })
            .Select(g => new TopPageData
            {
                Url = g.Key.Url,
                Title = g.Key.PageTitle ?? g.Key.Url,
                Views = g.Count(),
                UniqueVisitors = g.Select(e => e.SessionId).Distinct().Count()
            })
            .OrderByDescending(p => p.Views)
            .Take(limit)
            .ToListAsync();

        return topPages;
    }

    public async Task<List<ReferrerData>> GetTopReferrersAsync(DateTime startDate, DateTime endDate, int limit = 10)
    {
        var referrers = await _context.AnalyticsEvents
            .Where(e => e.EventType == "PageView"
                && e.Timestamp >= startDate
                && e.Timestamp <= endDate
                && !string.IsNullOrEmpty(e.Referrer))
            .GroupBy(e => e.Referrer)
            .Select(g => new ReferrerData
            {
                Referrer = g.Key!,
                Count = g.Count()
            })
            .OrderByDescending(r => r.Count)
            .Take(limit)
            .ToListAsync();

        return referrers;
    }

    public async Task<List<CountryData>> GetVisitorsByCountryAsync(DateTime startDate, DateTime endDate)
    {
        var countries = await _context.AnalyticsEvents
            .Where(e => e.EventType == "PageView"
                && e.Timestamp >= startDate
                && e.Timestamp <= endDate
                && !string.IsNullOrEmpty(e.Country))
            .GroupBy(e => e.Country)
            .Select(g => new CountryData
            {
                Country = g.Key!,
                Count = g.Count()
            })
            .OrderByDescending(c => c.Count)
            .ToListAsync();

        return countries;
    }

    public async Task<List<BrowserData>> GetBrowserStatsAsync(DateTime startDate, DateTime endDate)
    {
        var events = await _context.AnalyticsEvents
            .Where(e => e.EventType == "PageView" && e.Timestamp >= startDate && e.Timestamp <= endDate)
            .ToListAsync();

        var browsers = events
            .Select(e => ExtractBrowser(e.UserAgent))
            .GroupBy(b => b)
            .Select(g => new BrowserData
            {
                Browser = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(b => b.Count)
            .ToList();

        return browsers;
    }

    public async Task<List<DeviceData>> GetDeviceStatsAsync(DateTime startDate, DateTime endDate)
    {
        var events = await _context.AnalyticsEvents
            .Where(e => e.EventType == "PageView" && e.Timestamp >= startDate && e.Timestamp <= endDate)
            .ToListAsync();

        var devices = events
            .Select(e => DetermineDeviceType(e.UserAgent, e.ScreenWidth))
            .GroupBy(d => d)
            .Select(g => new DeviceData
            {
                Device = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(d => d.Count)
            .ToList();

        return devices;
    }

    private string ExtractBrowser(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";

        if (userAgent.Contains("Edg")) return "Edge";
        if (userAgent.Contains("Chrome")) return "Chrome";
        if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) return "Safari";
        if (userAgent.Contains("Firefox")) return "Firefox";
        if (userAgent.Contains("Opera") || userAgent.Contains("OPR")) return "Opera";

        return "Other";
    }

    private string DetermineDeviceType(string? userAgent, int? screenWidth)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";

        if (userAgent.Contains("Mobile") || userAgent.Contains("Android"))
            return "Mobile";

        if (userAgent.Contains("Tablet") || userAgent.Contains("iPad"))
            return "Tablet";

        if (screenWidth.HasValue && screenWidth < 768)
            return "Mobile";

        return "Desktop";
    }
}

// Data models
public class AnalyticsSummary
{
    public long TotalPageViews { get; set; }
    public int UniqueVisitors { get; set; }
    public long TotalRequests { get; set; }
    public long BandwidthUsed { get; set; }
    public long ThreatsBlocked { get; set; }
    public double AverageCacheHitRate { get; set; }
}

public class PageViewData
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class TopPageData
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Views { get; set; }
    public int UniqueVisitors { get; set; }
}

public class ReferrerData
{
    public string Referrer { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class CountryData
{
    public string Country { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class BrowserData
{
    public string Browser { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DeviceData
{
    public string Device { get; set; } = string.Empty;
    public int Count { get; set; }
}
