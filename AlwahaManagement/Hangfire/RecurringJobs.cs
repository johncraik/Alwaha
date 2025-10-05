using Hangfire;

namespace AlwahaManagement.Hangfire;

public class RecurringJobs
{
    private readonly TimeZoneInfo _tz = TimeZoneInfo.Local;
    
    public void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<AuditCleanupJob>("AUDIT_CLEANUP",
            x => x.Cleanup(),
            Cron.Monthly,
            new RecurringJobOptions{TimeZone = _tz});

        RecurringJob.AddOrUpdate<CloudflareSyncJob>("CLOUDFLARE_SYNC",
            x => x.ExecuteAsync(),
            Cron.Hourly,
            new RecurringJobOptions{TimeZone = _tz});

        RecurringJob.AddOrUpdate<AnalyticsCleanupJob>("ANALYTICS_CLEANUP",
            x => x.ExecuteAsync(),
            Cron.Weekly,
            new RecurringJobOptions{TimeZone = _tz});
    }
}