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

    public async Task<List<ItemTag>> GetItemTagsAsync()
        => await _context.ItemTags
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync();

    public async Task<ItemTag?> GetItemTagAsync(string id)
        => await _context.ItemTags
            .FirstOrDefaultAsync(t => t.TagId == id);

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
}