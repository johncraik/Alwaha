using AlwahaManagement.Services;

namespace AlwahaManagement.Hangfire;

public class CloudflareSyncJob
{
    private readonly CloudflareAnalyticsService _cloudflareService;
    private readonly ILogger<CloudflareSyncJob> _logger;

    public CloudflareSyncJob(CloudflareAnalyticsService cloudflareService, ILogger<CloudflareSyncJob> logger)
    {
        _cloudflareService = cloudflareService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        try
        {
            _logger.LogInformation("Starting Cloudflare analytics sync");

            // Sync last 7 days
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-7);

            var success = await _cloudflareService.SyncCloudflareDataAsync(startDate, endDate);

            if (success)
            {
                _logger.LogInformation("Cloudflare analytics sync completed successfully");
            }
            else
            {
                _logger.LogWarning("Cloudflare analytics sync failed or returned no data");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Cloudflare analytics sync");
        }
    }
}
