using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaSite.Pages;

public class Menu : PageModel
{
    private readonly MenuService _menuService;
    private readonly BundleService _bundleService;

    public Menu(MenuService menuService,
        BundleService bundleService)
    {
        _menuService = menuService;
        _bundleService = bundleService;
    }
    
    public List<(ItemType Type, List<MenuItem> Items, List<MenuItem> Sets, List<BundleItem> Bundles)> FullMenu { get; set; } 
    
    public async Task OnGet()
    {
        var items = await _menuService.GetMenuItemsAsync();
        var sets = await _menuService.GetMenuItemsAsync(getSets: true);
        var bundles = await _bundleService.GetBundleItemsAsync();

        // Get all unique item types
        var allTypes = items.Select(i => i.Key)
            .Concat(sets.Select(s => s.Key))
            .Concat(bundles.Select(b => b.Key))
            .Distinct()
            .OrderBy(t => t.Order)
            .ToList();

        // Combine items, sets, and bundles by item type
        FullMenu = allTypes.Select(type => (
            Type: type,
            Items: items.FirstOrDefault(g => g.Key.ItemTypeId == type.ItemTypeId)?.ToList() ?? [],
            Sets: sets.FirstOrDefault(g => g.Key.ItemTypeId == type.ItemTypeId)?.ToList() ?? [],
            Bundles: bundles.FirstOrDefault(g => g.Key.ItemTypeId == type.ItemTypeId)?.ToList() ?? []
        )).ToList();
    }
}