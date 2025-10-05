using AlwahaLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace AlwahaLibrary.Data;

public class AlwahaDbContext : DbContext
{
    public AlwahaDbContext(DbContextOptions<AlwahaDbContext> options)
        : base(options)
    {
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = new List<AuditEntry>();
        var entries = ChangeTracker.Entries<AuditModel>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry
            {
                TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name
            };

            // Get the primary key ID property (find first property ending with "Id" that has [Key] attribute or matches entity name)
            var entityType = entry.Entity.GetType();
            var idProperty = entityType.GetProperties()
                .FirstOrDefault(p => p.Name == $"{entityType.Name}Id" ||
                                     (p.Name.EndsWith("Id") && p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), false).Any()));

            auditEntry.EntityId = idProperty?.GetValue(entry.Entity)?.ToString() ?? "Unknown";

            if (entry.State == EntityState.Added)
            {
                auditEntry.AuditAction = AuditAction.CREATE;
                auditEntry.UserId = entry.Entity.CreatedBy;
                auditEntry.Date = entry.Entity.CreatedDate;
            }
            else if (entry.State == EntityState.Modified)
            {
                // Determine if it's Edit, Delete, or Restore by checking which fields changed
                var restoredDateProp = entry.Property(nameof(AuditModel.RestoredDate));
                var deletedDateProp = entry.Property(nameof(AuditModel.DeletedDate));
                var updatedDateProp = entry.Property(nameof(AuditModel.UpdatedDate));

                if (restoredDateProp.IsModified && entry.Entity.RestoredDate.HasValue)
                {
                    auditEntry.AuditAction = AuditAction.RESTORE;
                    auditEntry.UserId = entry.Entity.RestoredBy ?? "Unknown";
                    auditEntry.Date = entry.Entity.RestoredDate.Value;
                }
                else if (deletedDateProp.IsModified && entry.Entity.DeletedDate.HasValue)
                {
                    auditEntry.AuditAction = AuditAction.DELETE;
                    auditEntry.UserId = entry.Entity.DeletedBy ?? "Unknown";
                    auditEntry.Date = entry.Entity.DeletedDate.Value;
                }
                else if (updatedDateProp.IsModified && entry.Entity.UpdatedDate.HasValue)
                {
                    auditEntry.AuditAction = AuditAction.EDIT;
                    auditEntry.UserId = entry.Entity.UpdatedBy ?? "Unknown";
                    auditEntry.Date = entry.Entity.UpdatedDate.Value;
                }
                else
                {
                    // Fallback - something else changed
                    continue;
                }
            }

            auditEntries.Add(auditEntry);
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Any())
        {
            await AuditEntries.AddRangeAsync(auditEntries, cancellationToken);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<ItemToSet> ItemToSets { get; set; }
    public DbSet<ItemType> ItemTypes { get; set; }
    public DbSet<BundleItem> BundleItems { get; set; }
    public DbSet<ItemTag> ItemTags { get; set; }
    public DbSet<ItemToTag> ItemToTags { get; set; }
    public DbSet<AuditEntry> AuditEntries { get; set; }
    public DbSet<AnalyticsEvent> AnalyticsEvents { get; set; }
    public DbSet<CloudflareAnalytics> CloudflareAnalytics { get; set; }
 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure composite primary key for ItemToSet
        modelBuilder.Entity<ItemToSet>()
            .HasKey(its => new { its.ItemId, its.SetId });

        // Configure relationships for ItemToSet (many-to-many self-referencing)
        modelBuilder.Entity<ItemToSet>()
            .HasOne(its => its.MenuItem)
            .WithMany()
            .HasForeignKey(its => its.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ItemToSet>()
            .HasOne(its => its.MenuSet)
            .WithMany(m => m.ItemsToSets)
            .HasForeignKey(its => its.SetId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade to avoid multiple cascade paths

        // Configure composite primary key for ItemToTag
        modelBuilder.Entity<ItemToTag>()
            .HasKey(it => new { it.ItemId, it.TagId });

        // Configure relationships for ItemToTag
        modelBuilder.Entity<ItemToTag>()
            .HasOne(it => it.MenuItem)
            .WithMany(m => m.ItemsToTags)
            .HasForeignKey(it => it.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ItemToTag>()
            .HasOne(it => it.ItemTag)
            .WithMany(t => t.ItemsToTags)
            .HasForeignKey(it => it.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship for BundleItem
        modelBuilder.Entity<BundleItem>()
            .HasOne(b => b.MenuItem)
            .WithMany(m => m.BundleItems)
            .HasForeignKey(b => b.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship for MenuItem and ItemType
        modelBuilder.Entity<MenuItem>()
            .HasOne(m => m.ItemType)
            .WithMany()
            .HasForeignKey(m => m.ItemTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}