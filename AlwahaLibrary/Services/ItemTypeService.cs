using AlwahaLibrary.Data;
using AlwahaLibrary.Helpers;
using AlwahaLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace AlwahaLibrary.Services;

public class ItemTypeService
{
    private readonly AlwahaDbContext _context;
    private readonly UserInfo _userInfo;

    public ItemTypeService(AlwahaDbContext context,
        UserInfo userInfo)
    {
        _context = context;
        _userInfo = userInfo;
    }
    
    
    public async Task<List<ItemType>> GetItemTypesAsync()
        => await _context.ItemTypes
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Order)
            .ToListAsync();

    public async Task<ItemType?> GetItemTypeAsync(string id)
        => await _context.ItemTypes
            .FirstOrDefaultAsync(t => t.ItemTypeId == id);

    private async Task ValidateItemType(ItemType itemType, ModelStateWrapper modelState)
    {
        var res = await _context.ItemTypes.AnyAsync(it => it.Name == itemType.Name && it.ItemTypeId != itemType.ItemTypeId);
        if (res)
        {
            modelState.AddModelError(nameof(itemType.Name), "An item type with that name already exists.");
        }
    }

    public async Task<bool> TryCreateItemTypeAsync(ItemType itemType, ModelStateWrapper modelState)
    {
        var authorised = _userInfo.CanCreate();
        if (!authorised)
        {
            modelState.AddModelError(null, "You are not authorised to create item types.");
            return false;
        }
        
        await ValidateItemType(itemType, modelState);
        if (!modelState.IsValid) return false;
        
        itemType.Order = await _context.ItemTypes.MaxAsync(it => it.Order) + 1;
        itemType.FillCreated(_userInfo.UserId ?? "System");
        _context.ItemTypes.Add(itemType);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryUpdateItemTypeAsync(ItemType itemType, ModelStateWrapper modelState)
    {
        var authorised = _userInfo.CanEdit();
        if (!authorised)
        {
            modelState.AddModelError(null, "You are not authorised to modify item types.");
            return false;
        }
        
        await ValidateItemType(itemType, modelState);
        if (!modelState.IsValid) return false;
        
        itemType.FillUpdated(_userInfo.UserId ?? "System");
        _context.ItemTypes.Update(itemType);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> TryDeleteItemTypeAsync(ItemType itemType)
    {
        if(_userInfo.UserId == null) return false;
        
        var authorised = _userInfo.CanDelete();
        if (!authorised) return false;
        
        itemType.FillDeleted(_userInfo.UserId);
        _context.ItemTypes.Update(itemType);
        await _context.SaveChangesAsync();
        return true;
    }
}