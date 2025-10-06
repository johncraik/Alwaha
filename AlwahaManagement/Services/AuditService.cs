using AlwahaLibrary.Data;
using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using AlwahaManagement.Data;
using AlwahaManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace AlwahaManagement.Services;

public class AuditService
{
    private readonly AlwahaDbContext _context;

    public AuditService(AlwahaDbContext context,
        ApplicationDbContext authContext,
        UserInfo userInfo)
    {
        _context = context;
    }

    // Get audit entries for a specific entity
    public async Task<(List<AuditEntry> Entries, int Total)> GetAuditEntriesForEntityAsync(string tableName, string entityId, int page = 1, int pageSize = 20)
    {
        var query = _context.AuditEntries
            .Where(ae => ae.TableName == tableName && ae.EntityId == entityId)
            .OrderByDescending(ae => ae.Date);

        var total = await query.CountAsync();
        var entries = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (entries, total);
    }

    // Get audit entries for a specific user
    public async Task<(List<AuditEntry> Entries, int Total)> GetAuditEntriesForUserAsync(string userId, int page = 1, int pageSize = 20)
    {
        var query = _context.AuditEntries
            .Where(ae => ae.UserId == userId)
            .OrderByDescending(ae => ae.Date);

        var total = await query.CountAsync();
        var entries = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (entries, total);
    }

    // Get the entity name for display
    public async Task<string?> GetEntityNameAsync(string tableName, string entityId)
    {
        // Special case for BundleItem - need to load MenuItem to get name
        if (tableName == "BundleItems")
        {
            var bundleItem = await _context.BundleItems
                .Include(bi => bi.ItemType)
                .FirstOrDefaultAsync(bi => bi.BundleId == entityId);
            return bundleItem?.ItemType?.Name;
        }

        var entity = await GetEntityByTableNameAndIdAsync(tableName, entityId);
        if (entity == null) return null;

        // Try to get the Name property via reflection
        var nameProperty = entity.GetType().GetProperty("Name");
        return nameProperty?.GetValue(entity)?.ToString();
    }

    // Get all table names that have auditing
    public List<string> GetAuditedTableNames()
    {
        return _context.AuditEntries
            .Select(ae => ae.TableName)
            .Distinct()
            .OrderBy(tn => tn)
            .ToList();
    }

    // Get all entities from a specific table (for dropdown selection)
    public async Task<List<(string Id, string Name)>> GetEntitiesFromTableAsync(string tableName)
    {
        var entityType = GetEntityTypeFromTableName(tableName);
        if (entityType == null) return new List<(string, string)>();

        var setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!
            .MakeGenericMethod(entityType);

        var dbSet = setMethod.Invoke(_context, null) as IQueryable<object>;
        if (dbSet == null) return new List<(string, string)>();

        var entities = await ((IQueryable<AuditModel>)dbSet).ToListAsync();

        return entities.Select(e =>
        {
            var idProp = e.GetType().GetProperties()
                .FirstOrDefault(p => p.Name.EndsWith("Id") &&
                               p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), false).Any());
            var nameProp = e.GetType().GetProperty("Name");

            var id = idProp?.GetValue(e)?.ToString() ?? "Unknown";
            var name = nameProp?.GetValue(e)?.ToString() ?? "Unnamed";

            return (id, name);
        }).ToList();
    }

    private async Task<object?> GetEntityByTableNameAndIdAsync(string tableName, string entityId)
    {
        var entityType = GetEntityTypeFromTableName(tableName);
        if (entityType == null) return null;

        // Use reflection to call Set<T>().FindAsync(entityId)
        var setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!
            .MakeGenericMethod(entityType);

        var dbSet = setMethod.Invoke(_context, null);

        var findMethod = dbSet!.GetType().GetMethod(nameof(DbSet<object>.FindAsync), [typeof(object[])]);
        var findTask = findMethod!.Invoke(dbSet, [new object[] { entityId }]);

        // FindAsync returns ValueTask<T>, so we need to await it
        var asTaskMethod = findTask!.GetType().GetMethod(nameof(ValueTask<object>.AsTask));
        var task = (Task)asTaskMethod!.Invoke(findTask, null)!;

        await task.ConfigureAwait(false);

        var resultProperty = task.GetType().GetProperty(nameof(Task<object>.Result));
        return resultProperty!.GetValue(task);
    }

    private Type? GetEntityTypeFromTableName(string tableName)
    {
        return tableName switch
        {
            "MenuItems" => typeof(MenuItem),
            "ItemTypes" => typeof(ItemType),
            "ItemTags" => typeof(ItemTag),
            "BundleItems" => typeof(BundleItem),
            _ => null
        };
    }
}