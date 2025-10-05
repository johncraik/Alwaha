using AlwahaLibrary.Data;
using AlwahaManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace AlwahaManagement.Hangfire;

public class AnalyticsCleanupJob
{
    private readonly AlwahaDbContext _context;
    private readonly SettingsService _settingsService;
    private readonly ILogger<AnalyticsCleanupJob> _logger;

    public AnalyticsCleanupJob(AlwahaDbContext context, SettingsService settingsService, ILogger<AnalyticsCleanupJob> logger)
    {
        _context = context;
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        try
        {
            _logger.LogInformation("Starting analytics cleanup");

            var retentionMonths = await _settingsService.GetSettingAsync<int>("AnalyticsRetentionMonths");
            if (retentionMonths == 0)
            {
                retentionMonths = 6; // Default to 6 months if not set
            }

            var cutoffDate = DateTime.UtcNow.AddMonths(-retentionMonths);

            // Delete old analytics events
            var deletedEvents = await _context.AnalyticsEvents
                .Where(e => e.Timestamp < cutoffDate)
                .ExecuteDeleteAsync();

            // Delete old Cloudflare data
            var deletedCloudflare = await _context.CloudflareAnalytics
                .Where(c => c.Date < cutoffDate.Date)
                .ExecuteDeleteAsync();

            _logger.LogInformation($"Analytics cleanup completed. Deleted {deletedEvents} events and {deletedCloudflare} Cloudflare records");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during analytics cleanup");
        }
    }
}
