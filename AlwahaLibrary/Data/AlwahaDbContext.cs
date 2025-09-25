using AlwahaLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace AlwahaLibrary.Data;

public class AlwahaDbContext : DbContext
{
    public AlwahaDbContext(DbContextOptions<AlwahaDbContext> options)
        : base(options)
    {
    }

    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<ItemToSet> ItemToSets { get; set; }
    public DbSet<ItemType> ItemTypes { get; set; }
    public DbSet<BundleItem> BundleItems { get; set; }
    public DbSet<ItemTag> ItemTags { get; set; }
    public DbSet<ItemToTag> ItemToTags { get; set; }

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
            .WithMany()
            .HasForeignKey(its => its.SetId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade to avoid multiple cascade paths

        // Configure composite primary key for ItemToTag
        modelBuilder.Entity<ItemToTag>()
            .HasKey(it => new { it.ItemId, it.TagId });

        // Configure relationships for ItemToTag
        modelBuilder.Entity<ItemToTag>()
            .HasOne(it => it.MenuItem)
            .WithMany()
            .HasForeignKey(it => it.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ItemToTag>()
            .HasOne(it => it.ItemTag)
            .WithMany()
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