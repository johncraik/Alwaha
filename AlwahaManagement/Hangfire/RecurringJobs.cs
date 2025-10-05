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
    }
}