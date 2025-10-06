using AlwahaLibrary.Data;
using AlwahaLibrary.Helpers;
using AlwahaLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace AlwahaLibrary.Services;

public class BundleService
{
    private readonly AlwahaDbContext _context;
    private readonly UserInfo _userInfo;

    public BundleService(AlwahaDbContext context, UserInfo userInfo)
    {
        _context = context;
        _userInfo = userInfo;
    }

    public async Task<List<IGrouping<ItemType, BundleItem>>> GetBundleItemsAsync(
        string search = "",
        bool isRestore = false)
    {
        var query = _context.BundleItems
            .Where(b => b.IsDeleted == isRestore)
            .Include(b => b.ItemType)
            .AsQueryable();

        if(!string.IsNullOrEmpty(search))
        {
            query = query.Where(b => b.ItemType.Name.Contains(search));
        }

        var bundles = await query
            .OrderBy(b => b.ItemType.Order)
            .ToListAsync();

        return bundles
            .GroupBy(b => b.ItemType)
            .OrderBy(g => g.Key?.Order ?? int.MaxValue)
            .ToList();
    }

    public async Task<BundleItem?> GetBundleItemAsync(string id)
        => await _context.BundleItems
            .Include(b => b.ItemType)
            .FirstOrDefaultAsync(b => b.BundleId == id);

    private async Task ValidateBundleItemAsync(BundleItem bundleItem, ModelStateWrapper modelState)
    {
        if (bundleItem.Quantity <= 0)
        {
            modelState.AddModelError(nameof(bundleItem.Quantity), "Quantity must be greater than 0.");
        }

        if (bundleItem.Price <= 0)
        {
            modelState.AddModelError(nameof(bundleItem.Price), "Price must be greater than 0.");
        }

        var typeExists = await _context.ItemTypes.AnyAsync(m => m.ItemTypeId == bundleItem.ItemTypeId && !m.IsDeleted);
        if (!typeExists)
        {
            modelState.AddModelError(nameof(bundleItem.ItemTypeId), "Invalid item type.");
        }
        
        var sameBundle = await _context.BundleItems.AnyAsync(b => b.BundleId != bundleItem.BundleId 
                                                                  && b.ItemTypeId == bundleItem.ItemTypeId 
                                                                  && b.Quantity == bundleItem.Quantity);
        if (sameBundle)
        {
            modelState.AddModelError(nameof(bundleItem.ItemTypeId), "This item is already in this bundle.");
        }
    }

    public async Task<bool> TryCreateBundleItemAsync(BundleItem bundleItem, ModelStateWrapper modelState)
    {
        var authorised = _userInfo.CanCreate();
        if (!authorised)
        {
            modelState.AddModelError(null, "You are not authorised to create bundle items.");
            return false;
        }

        await ValidateBundleItemAsync(bundleItem, modelState);
        if (!modelState.IsValid) return false;

        bundleItem.FillCreated(_userInfo.UserId ?? "System");
        await _context.BundleItems.AddAsync(bundleItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryUpdateBundleItemAsync(BundleItem bundleItem, ModelStateWrapper modelState)
    {
        var authorised = _userInfo.CanEdit();
        if (!authorised)
        {
            modelState.AddModelError(null, "You are not authorised to modify bundle items.");
            return false;
        }

        await ValidateBundleItemAsync(bundleItem, modelState);
        if (!modelState.IsValid) return false;

        bundleItem.FillUpdated(_userInfo.UserId ?? "System");
        _context.BundleItems.Update(bundleItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryDeleteBundleItemAsync(BundleItem bundleItem)
    {
        if (_userInfo.UserId == null) return false;

        var authorised = _userInfo.CanDelete();
        if (!authorised) return false;

        bundleItem.FillDeleted(_userInfo.UserId);
        _context.BundleItems.Update(bundleItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryRestoreBundleItemAsync(BundleItem bundleItem)
    {
        if (_userInfo.UserId == null) return false;

        var authorised = _userInfo.CanRestore();
        if (!authorised) return false;

        bundleItem.FillRestored(_userInfo.UserId);
        _context.BundleItems.Update(bundleItem);
        await _context.SaveChangesAsync();
        return true;
    }
}