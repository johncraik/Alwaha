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
}