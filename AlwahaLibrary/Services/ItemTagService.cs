using AlwahaLibrary.Data;
using AlwahaLibrary.Helpers;
using AlwahaLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace AlwahaLibrary.Services;

public class ItemTagService
{
    private readonly AlwahaDbContext _context;
    private readonly UserInfo _userInfo;

    public ItemTagService(AlwahaDbContext context,
        UserInfo userInfo)
    {
        _context = context;
        _userInfo = userInfo;
    }

    public async Task<List<ItemTag>> GetItemTagsAsync(bool isRestore = false)
        => await _context.ItemTags
            .Where(t => t.IsDeleted == isRestore)
            .OrderBy(t => t.Name)
            .ToListAsync();

    public async Task<ItemTag?> GetItemTagAsync(string id)
        => await _context.ItemTags
            .FirstOrDefaultAsync(t => t.TagId == id);

    public async Task<List<string>> GetItemTagIdsAsync(string itemId)
        => await _context.ItemToTags
            .Where(it => it.ItemId == itemId)
            .Select(it => it.TagId)
            .ToListAsync();

    public async Task<List<ItemTag>> GetItemTagsForItemAsync(string itemId)
    {
        var ids = await GetItemTagIdsAsync(itemId);
        if (ids.Count == 0) return [];
        
        return await _context.ItemTags
            .Where(t => ids.Contains(t.TagId))
            .ToListAsync();
    }

    private async Task ValidateItemTag(ItemTag itemTag, ModelStateWrapper modelState)
    {
        var res = await _context.ItemTags.AnyAsync(it => it.Name == itemTag.Name && it.TagId != itemTag.TagId);
        if (res)
        {
            modelState.AddModelError(nameof(itemTag.Name), "An item tag with that name already exists.");
        }
    }

    public async Task<bool> TryCreateItemTagAsync(ItemTag itemTag, ModelStateWrapper modelState)
    {
        var authorised = _userInfo.CanCreate();
        if (!authorised)
        {
            modelState.AddModelError(null, "You are not authorised to create item tags.");
            return false;
        }

        await ValidateItemTag(itemTag, modelState);
        if (!modelState.IsValid) return false;

        itemTag.FillCreated(_userInfo.UserId ?? "System");
        _context.ItemTags.Add(itemTag);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryUpdateItemTagAsync(ItemTag itemTag, ModelStateWrapper modelState)
    {
        var authorised = _userInfo.CanEdit();
        if (!authorised)
        {
            modelState.AddModelError(null, "You are not authorised to modify item tags.");
            return false;
        }

        await ValidateItemTag(itemTag, modelState);
        if (!modelState.IsValid) return false;

        itemTag.FillUpdated(_userInfo.UserId ?? "System");
        _context.ItemTags.Update(itemTag);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryDeleteItemTagAsync(ItemTag itemTag)
    {
        if(_userInfo.UserId == null) return false;

        var authorised = _userInfo.CanDelete();
        if (!authorised) return false;

        itemTag.FillDeleted(_userInfo.UserId);
        _context.ItemTags.Update(itemTag);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryRestoreItemTagAsync(ItemTag itemTag)
    {
        if(_userInfo.UserId == null) return false;

        var authorised = _userInfo.CanRestore();
        if (!authorised) return false;

        itemTag.FillRestored(_userInfo.UserId);
        _context.ItemTags.Update(itemTag);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryModifyItemTagsAsync(string itemId, List<string> tagIds)
    {
        var item = await _context.MenuItems.FirstOrDefaultAsync(i => i.ItemId == itemId);
        if (item == null) return false;
        
        var tags = await _context.ItemTags
            .Where(t => tagIds.Contains(t.TagId) && !t.IsDeleted)
            .Select(t => t.TagId)
            .ToListAsync();
        var currentTags = await _context.ItemToTags
            .Where(it => it.ItemId == itemId)
            .ToListAsync();
        
        var tagsIdToAdd = tags.Where(t => !currentTags.Select(ct => ct.TagId).Contains(t)).ToList();
        var tagsToRemove = currentTags.Where(ct => !tags.Contains(ct.TagId)).ToList();
        
        var tagsToAdd = tagsIdToAdd.Select(t => new ItemToTag
        {
            ItemId = itemId, 
            TagId = t
        }).ToList();
        await _context.ItemToTags.AddRangeAsync(tagsToAdd);
        _context.ItemToTags.RemoveRange(tagsToRemove);
        await _context.SaveChangesAsync();
        return true;
    }
    
    // public async Task<bool> TryModifySetTagsAsync(string setId, List<string> itemIds)
    // {
    //     var items = await _context.MenuItems.Where(i => itemIds.Contains(i.ItemId)).ToListAsync();
    //     if(items.Count == 0) return false;
    //     
    //     var tagIds = await _context.ItemToTags
    //         .Where(itt => items.Select(i => i.ItemId).Contains(itt.ItemId))
    //         .Select(itt => itt.TagId)
    //         .ToListAsync();
    //     return await TryModifyItemTagsAsync(setId, tagIds);
    // }
}