using AlwahaLibrary.Data;
using AlwahaLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace AlwahaLibrary.Services;

public class ItemTypeService
{
    private readonly AlwahaDbContext _context;

    public ItemTypeService(AlwahaDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<ItemType>> GetItemTypesAsync()
        => await _context.ItemTypes
            .Where(t => !t.IsDeleted)
            .ToListAsync();

    public async Task<ItemType?> GetItemTypeAsync(string id)
        => await _context.ItemTypes
            .FirstOrDefaultAsync(t => t.ItemTypeId == id);
}