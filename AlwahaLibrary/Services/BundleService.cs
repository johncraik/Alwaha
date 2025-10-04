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
        bool showUnavailable = false)
    {
        var query = _context.BundleItems
            .Where(b => !b.IsDeleted)
            .Include(b => b.MenuItem)
            .ThenInclude(m => m.ItemType)
            .Include(b => b.MenuItem.ItemsToTags)
            .ThenInclude(itt => itt.ItemTag)
            .AsQueryable();

        if (!showUnavailable)
        {
            query = query.Where(b => b.MenuItem.IsAvailable);
        }

        if(!string.IsNullOrEmpty(search))
        {
            query = query.Where(b => b.MenuItem.Name.Contains(search)
                                     || (!string.IsNullOrEmpty(b.MenuItem.Description) && b.MenuItem.Description.Contains(search)));
        }

        var bundles = await query
            .OrderBy(b => b.MenuItem.ItemType.Order)
            .ThenBy(b => b.MenuItem.Name)
            .ToListAsync();

        return bundles
            .GroupBy(b => b.MenuItem.ItemType)
            .OrderBy(g => g.Key?.Order ?? int.MaxValue)
            .ToList();
    }

    public async Task<BundleItem?> GetBundleItemAsync(string id)
        => await _context.BundleItems
            .Include(b => b.MenuItem)
            .ThenInclude(m => m.ItemType)
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

        var itemExists = await _context.MenuItems.AnyAsync(m => m.ItemId == bundleItem.ItemId && !m.IsDeleted);
        if (!itemExists)
        {
            modelState.AddModelError(nameof(bundleItem.ItemId), "Invalid menu item.");
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
}