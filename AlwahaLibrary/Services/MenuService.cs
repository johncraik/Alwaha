using AlwahaLibrary.Data;
using AlwahaLibrary.Helpers;
using AlwahaLibrary.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace AlwahaLibrary.Services;

public class MenuService
{
    private readonly AlwahaDbContext _context;
    private readonly ItemTagService _itemTagService;
    private readonly UserInfo _userInfo;

    public MenuService(AlwahaDbContext context,
        ItemTagService itemTagService,
        UserInfo userInfo)
    {
        _context = context;
        _itemTagService = itemTagService;
        _userInfo = userInfo;
    }


    
    public async Task<List<IGrouping<ItemType, MenuItem>>> GetMenuItemsAsync(
        string search = "", 
        bool showUnavailable = false,
        bool getSets = false)
    {
        var query = _context.MenuItems
            .Where(i => !i.IsDeleted)
            .Include(i => i.ItemType)
            .Include(i => i.ItemsToTags)
            .ThenInclude(itt => itt.ItemTag)
            .AsQueryable();
        if (getSets)
        {
            query = query.Where(i => i.IsSet)
                .Include(i => i.ItemsToSets)
                .ThenInclude(its => its.MenuItem)
                .ThenInclude(i => i.ItemType);
        }
        else
        {
            query = query.Where(i => !i.IsSet);
        }
        
        if (!showUnavailable) query = query.Where(i => i.IsAvailable);

        if(!string.IsNullOrEmpty(search))
        {
            query = query.Where(i => i.Name.Contains(search)
                                     || (!string.IsNullOrEmpty(i.Description) && i.Description.Contains(search)));
        }

        var items = await query.OrderBy(i => i.ItemType.Order).ThenBy(i => i.Name).ToListAsync();
        return items.GroupBy(i => i.ItemType).OrderBy(g => g.Key?.Order ?? int.MaxValue).ToList();
    }
    

    // public async Task<List<IGrouping<ItemType, MenuItem>>> GetSetsAsync(
    //     string search = "",
    //     bool showUnavailable = false)
    // {
    //     //Get the menu sets:
    //     var query = _context.MenuItems
    //         .Where(i => i.IsSet && !i.IsDeleted)
    //         .Include(i => i.ItemType)
    //         .Include(i => i.ItemsToSets)
    //         .ThenInclude(its => its.MenuItem)
    //         .Include(i => i.ItemsToTags)
    //         .ThenInclude(itt => itt.ItemTag)
    //         .AsQueryable();
    //     if (!showUnavailable) query = query.Where(i => i.IsAvailable);
    //
    //     //Search the menu sets:
    //     if (!string.IsNullOrEmpty(search))
    //     {
    //         query = query.Where(i => i.Name.Contains(search)
    //                                  || (!string.IsNullOrEmpty(i.Description) && i.Description.Contains(search)));
    //     }
    //     
    //     //Convert to list:
    //     var sets = await query.OrderBy(s => s.ItemType.Order).ToListAsync();
    //
    //     
    //     //Grab the menu items in the menu set:
    //     var setItems = _context.ItemToSets
    //         .Include(its => its.MenuItem)
    //         .Where(its => !its.MenuItem.IsDeleted 
    //                       && menuSets.Select(s => s.ItemId).Contains(its.SetId))
    //         .AsQueryable();
    //     if (!showUnavailable) setItems = setItems.Where(its => its.MenuItem.IsAvailable);
    //     
    //     //Search the menu items:
    //     if (!string.IsNullOrEmpty(search))
    //     {
    //         setItems = setItems.Where(its => its.MenuItem.Name.Contains(search) 
    //                                         || (!string.IsNullOrEmpty(its.MenuItem.Description) && its.MenuItem.Description.Contains(search)));
    //     }
    //     
    //     
    //     //Convert sets and items into one list:
    //     return (from set in menuSets
    //         let setItemsList = setItems
    //             .Where(its => its.SetId == set.ItemId)
    //             .Select(its => its.MenuItem)
    //             .ToList()
    //         select (set, setItemsList)).ToList();
    // }


    public async Task<MenuItem?> GetMenuItemAsync(string id)
        => await _context.MenuItems
            .Include(i => i.ItemType)
            .Include(i => i.BundleItems)
            .Include(i => i.ItemsToSets)
            .FirstOrDefaultAsync(i => i.ItemId == id);

    public async Task<List<string>> GetMenuItemIdsInSetAsync(string setId)
    {
        var items = await _context.ItemToSets
            .Where(its => its.SetId == setId)
            .Select(its => its.MenuItem.ItemId)
            .ToListAsync();
        return items;
    }


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
        var authorised = _userInfo.CanCreate();
        if (!authorised)
        {
            modelState.AddModelError(null, "You are not authorised to create menu items.");
            return false;
        }
        
        await ValidateMenuItemAsync(menuItem, modelState);
        if (!modelState.IsValid) return false;
        
        menuItem.FillCreated(_userInfo.UserId ?? "System");
        menuItem.IsSet = false;
        menuItem.Colour = null;
        
        await _context.MenuItems.AddAsync(menuItem);
        await _context.SaveChangesAsync();
        return true;
    }
    

    public async Task<bool> TryUpdateMenuItemAsync(MenuItem menuItem, ModelStateWrapper modelState)
    {
        var authorised = _userInfo.CanEdit();
        if (!authorised)
        {
            modelState.AddModelError(null, "You are not authorised to modify menu items.");
            return false;
        }
        
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
        
        var authorised = _userInfo.CanDelete();
        if (!authorised) return false;
        
        menuItem.FillDeleted(_userInfo.UserId);
        _context.MenuItems.Update(menuItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryCreateSetAsync(MenuItem menuSet, List<string> menuItemIds, ModelStateWrapper modelState)
    {
        var authorised = _userInfo.CanCreate();
        if (!authorised) return false;
        
        await ValidateMenuItemAsync(menuSet, modelState);
        if (!modelState.IsValid) return false;
        
        menuSet.FillCreated(_userInfo.UserId ?? "System");
        menuSet.IsSet = true;
        menuSet.ImagePath = null;
        
        await _context.MenuItems.AddAsync(menuSet);
        await _context.SaveChangesAsync();

        var links = menuItemIds.Select(i => new ItemToSet
        {
            ItemId = i,
            SetId = menuSet.ItemId
        });
        await _context.ItemToSets.AddRangeAsync(links);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> TryUpdateSetAsync(MenuItem menuSet, List<string> menuItemIds, ModelStateWrapper modelState)
    {
        var authorised = _userInfo.CanEdit();
        if (!authorised) return false;
        
        await ValidateMenuItemAsync(menuSet, modelState);
        if (!modelState.IsValid) return false;
        
        menuSet.FillUpdated(_userInfo.UserId ?? "System");
        menuSet.IsSet = true;
        menuSet.ImagePath = null;
        
        _context.MenuItems.Update(menuSet);
        
        var existingLinks = await _context.ItemToSets
            .Where(its => its.SetId == menuSet.ItemId)
            .ToListAsync();
        var linksToRemove = existingLinks.Where(its => !menuItemIds.Contains(its.ItemId)).ToList();
        var linksToAdd = menuItemIds.Where(i => !existingLinks.Select(its => its.ItemId).Contains(i))
            .Select(i => new ItemToSet
            {
                ItemId = i,
                SetId = menuSet.ItemId
            }).ToList();
        
        _context.ItemToSets.RemoveRange(linksToRemove);
        await _context.ItemToSets.AddRangeAsync(linksToAdd);
        await _context.SaveChangesAsync();
        
        return true;
    }
}