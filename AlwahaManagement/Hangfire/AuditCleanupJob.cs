using AlwahaLibrary.Data;
using AlwahaManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace AlwahaManagement.Hangfire;

public class AuditCleanupJob
{
    private readonly AlwahaDbContext _context;
    private readonly SettingsService _settingsService;

    public AuditCleanupJob(AlwahaDbContext context,
        SettingsService settingsService)
    {
        _context = context;
        _settingsService = settingsService;
    }
    
    public async Task Cleanup()
    {
        var lifetime = await _settingsService.GetSettingAsync<int>("AuditLifetimeMonths");
        
        var oldEntries = await _context.AuditEntries
            .Where(ae => ae.Date <= DateTime.UtcNow.AddMonths(-lifetime))
            .ToListAsync();
        _context.AuditEntries.RemoveRange(oldEntries);
        await _context.SaveChangesAsync();
    }
}