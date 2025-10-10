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
        var cutoffDate = DateTime.UtcNow.AddMonths(-lifetime);
        var entriesToDelete = new List<AlwahaLibrary.Models.AuditEntry>();

        // 1. Cleanup per ENTITY (TableName + EntityId) - keep at least 50 per entity
        var entities = await _context.AuditEntries
            .Select(ae => new { ae.TableName, ae.EntityId })
            .Distinct()
            .ToListAsync();

        foreach (var entity in entities)
        {
            var entityEntries = await _context.AuditEntries
                .Where(ae => ae.TableName == entity.TableName && ae.EntityId == entity.EntityId)
                .OrderByDescending(ae => ae.Date)
                .ToListAsync();

            // Only delete if there are more than 50 entries for this entity
            if (entityEntries.Count > 50)
            {
                // Keep the 50 most recent, delete older ones that exceed the lifetime
                var oldEntries = entityEntries
                    .Skip(50)
                    .Where(ae => ae.Date <= cutoffDate)
                    .ToList();

                entriesToDelete.AddRange(oldEntries);
            }
        }

        // 2. Cleanup per USER - keep at least 50 per user
        var userIds = await _context.AuditEntries
            .Select(ae => ae.UserId)
            .Distinct()
            .ToListAsync();

        foreach (var userId in userIds)
        {
            var userEntries = await _context.AuditEntries
                .Where(ae => ae.UserId == userId)
                .OrderByDescending(ae => ae.Date)
                .ToListAsync();

            // Only delete if there are more than 50 entries for this user
            if (userEntries.Count > 50)
            {
                // Keep the 50 most recent, delete older ones that exceed the lifetime
                var oldEntries = userEntries
                    .Skip(50)
                    .Where(ae => ae.Date <= cutoffDate)
                    .ToList();

                entriesToDelete.AddRange(oldEntries);
            }
        }

        // Remove duplicates (in case an entry qualifies for deletion from both entity and user perspectives)
        var uniqueEntriesToDelete = entriesToDelete.Distinct().ToList();

        if (uniqueEntriesToDelete.Any())
        {
            _context.AuditEntries.RemoveRange(uniqueEntriesToDelete);
            await _context.SaveChangesAsync();
        }
    }
}