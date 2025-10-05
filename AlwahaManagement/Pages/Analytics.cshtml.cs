using AlwahaLibrary.Authentication;
using AlwahaManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages;

[Authorize(Roles = SystemRoles.Admin)]
public class AnalyticsModel : PageModel
{
    private readonly AnalyticsService _analyticsService;

    public AnalyticsModel(AnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public AnalyticsSummary Summary { get; set; } = new();
    public List<PageViewData> PageViewsData { get; set; } = new();
    public List<PageViewData> UniqueViewsData { get; set; } = new();
    public List<TopPageData> TopPages { get; set; } = new();
    public List<ReferrerData> TopReferrers { get; set; } = new();
    public List<CountryData> CountryData { get; set; } = new();
    public List<BrowserData> BrowserData { get; set; } = new();
    public List<DeviceData> DeviceData { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    public async Task OnGetAsync()
    {
        // Default to last 30 days if not provided
        var startDate = StartDate?.Date ?? DateTime.UtcNow.Date.AddDays(-30);
        var endDate = EndDate?.Date ?? DateTime.UtcNow.Date;

        // Ensure end date is end of day
        endDate = endDate.AddDays(1).AddTicks(-1);

        // Update properties for form display
        StartDate = startDate;
        EndDate = endDate.Date;

        Summary = await _analyticsService.GetSummaryAsync(startDate, endDate);
        PageViewsData = await _analyticsService.GetPageViewsOverTimeAsync(startDate, endDate);
        UniqueViewsData = await _analyticsService.GetUniqueVisitorsOverTimeAsync(startDate, endDate);
        TopPages = await _analyticsService.GetTopPagesAsync(startDate, endDate);
        TopReferrers = await _analyticsService.GetTopReferrersAsync(startDate, endDate);
        CountryData = await _analyticsService.GetVisitorsByCountryAsync(startDate, endDate);
        BrowserData = await _analyticsService.GetBrowserStatsAsync(startDate, endDate);
        DeviceData = await _analyticsService.GetDeviceStatsAsync(startDate, endDate);
    }
}
