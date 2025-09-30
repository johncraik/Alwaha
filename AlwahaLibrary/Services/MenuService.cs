using AlwahaLibrary.Data;
using AlwahaLibrary.Helpers;
using AlwahaLibrary.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace AlwahaLibrary.Services;

public class MenuService
{
    private readonly AlwahaDbContext _context;
    private readonly UserInfo _userInfo;

    public MenuService(AlwahaDbContext context,
        UserInfo userInfo)
    {
        _context = context;
        _userInfo = userInfo;
    }


    /// <summary>
    /// Retrieves a list of menu items based on the provided search criteria and availability filter.
    /// </summary>
    /// <param name="search">The search string to filter menu items by name or description.</param>
    /// <param name="showUnavailable">A boolean value indicating whether unavailable items should be included in the results. Defaults to false.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="MenuItem"/> objects matching the criteria.</returns>
    public async Task<List<MenuItem>> GetMenuItemsAsync(string search = "", bool showUnavailable = false)
    {
        var query = _context.MenuItems
            .Where(i => !i.IsSet && !i.IsDeleted)
            .Include(i => i.ItemType)
            .AsQueryable();
        if (!showUnavailable) query = query.Where(i => i.IsAvailable);

        if(!string.IsNullOrEmpty(search))
        {
            query = query.Where(i => i.Name.Contains(search) 
                                     || (!string.IsNullOrEmpty(i.Description) && i.Description.Contains(search)));
        }
        
        return await query.OrderBy(i => i.ItemType.Order).ThenBy(i => i.Name).ToListAsync();
    }

    public async Task<List<(MenuItem SetItem, List<MenuItem> Items)>> GetSetsAsync(
        string search = "",
        bool showUnavailable = false)
    {
        //Get the menu sets:
        var query = _context.MenuItems
            .Where(i => i.IsSet && !i.IsDeleted)
            .AsQueryable();
        if (!showUnavailable) query = query.Where(i => i.IsAvailable);

        //Search the menu sets:
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(i => i.Name.Contains(search)
                                     || (!string.IsNullOrEmpty(i.Description) && i.Description.Contains(search)));
        }
        
        //Convert to list:
        var menuSets = await query.ToListAsync();

        
        //Grab the menu items in the menu set:
        var setItems = _context.ItemToSets
            .Include(its => its.MenuItem)
            .Where(its => !its.MenuItem.IsDeleted 
                          && menuSets.Select(s => s.ItemId).Contains(its.SetId))
            .AsQueryable();
        if (!showUnavailable) setItems = setItems.Where(its => its.MenuItem.IsAvailable);
        
        //Search the menu items:
        if (!string.IsNullOrEmpty(search))
        {
            setItems = setItems.Where(its => its.MenuItem.Name.Contains(search) 
                                            || (!string.IsNullOrEmpty(its.MenuItem.Description) && its.MenuItem.Description.Contains(search)));
        }
        
        
        //Convert sets and items into one list:
        return (from set in menuSets
            let setItemsList = setItems
                .Where(its => its.SetId == set.ItemId)
                .Select(its => its.MenuItem)
                .ToList()
            select (set, setItemsList)).ToList();
    }


    public async Task<MenuItem?> GetMenuItemAsync(string id)
        => await _context.MenuItems
            .Include(i => i.ItemType)
            .Include(i => i.BundleItems)
            .Include(i => i.SetItems)
            .FirstOrDefaultAsync(i => i.ItemId == id);


    private async Task ValidateMenuItemAsync(MenuItem menuItem, ModelStateWrapper modelState)
    {
        var res = await _context.MenuItems.AnyAsync(i => i.Name == menuItem.Name 
                                                         && i.ItemId != menuItem.ItemId);
        if (res)
        {
            modelState.AddModelError(nameof(menuItem.Name), "A menu item with that name already exists.");
        }
        
        if(menuItem.Price <= 0)
        {
            modelState.AddModelError(nameof(menuItem.Price), "Price cannot be 0 or negative.");
        }

        res = await _context.ItemTypes.AnyAsync(it => it.ItemTypeId == menuItem.ItemTypeId);
        if (!res)
        {
            modelState.AddModelError(nameof(menuItem.ItemTypeId), "Invalid item type.");
        }
    }

    public async Task<bool> TryCreateMenuItemAsync(MenuItem menuItem, ModelStateWrapper modelState)
    {
        await ValidateMenuItemAsync(menuItem, modelState);
        if (!modelState.IsValid) return false;
        
        menuItem.FillCreated(_userInfo.UserId ?? "System");
        menuItem.IsSet = false;
        menuItem.Colour = null;
        
        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();
        return true;
    }
    

    public async Task<bool> TryUpdateMenuItemAsync(MenuItem menuItem, ModelStateWrapper modelState)
    {
        await ValidateMenuItemAsync(menuItem, modelState);
        if (!modelState.IsValid) return false;
        
        menuItem.FillUpdated(_userInfo.UserId ?? "System");
        menuItem.IsSet = false;
        menuItem.Colour = null;
        
        _context.MenuItems.Update(menuItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryDeleteMenuItemAsync(MenuItem menuItem)
    {
        if(_userInfo.UserId == null) return false;
        
        menuItem.FillDeleted(_userInfo.UserId);
        _context.MenuItems.Update(menuItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryCreateSetAsync(MenuItem menuItem, List<MenuItem> menuItems, ModelStateWrapper modelState)
    {
        await ValidateMenuItemAsync(menuItem, modelState);
        if (!modelState.IsValid) return false;
        
        menuItem.FillCreated(_userInfo.UserId ?? "System");
        menuItem.IsSet = true;
        menuItem.ImagePath = null;
        
        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();
        return true;
    }
}