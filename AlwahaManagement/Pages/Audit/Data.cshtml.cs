using AlwahaLibrary.Authentication;
using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Audit;

[Authorize(Roles = SystemRoles.Admin)]
public class Data : PageModel
{
    private readonly MenuService _menuService;
    private readonly ItemTypeService _itemTypeService;
    private readonly ItemTagService _itemTagService;
    private readonly BundleService _bundleService;

    public Data(MenuService menuService,
        ItemTypeService itemTypeService,
        ItemTagService itemTagService,
        BundleService bundleService)
    {
        _menuService = menuService;
        _itemTypeService = itemTypeService;
        _itemTagService = itemTagService;
        _bundleService = bundleService;
    }

    // MenuItems
    public List<IGrouping<ItemType, MenuItem>> ActiveMenuItems { get; set; } = [];
    public List<IGrouping<ItemType, MenuItem>> DeletedMenuItems { get; set; } = [];

    // Sets
    public List<IGrouping<ItemType, MenuItem>> ActiveSets { get; set; } = [];
    public List<IGrouping<ItemType, MenuItem>> DeletedSets { get; set; } = [];

    // Bundles
    public List<IGrouping<ItemType, BundleItem>> ActiveBundles { get; set; } = [];
    public List<IGrouping<ItemType, BundleItem>> DeletedBundles { get; set; } = [];

    // ItemTypes
    public List<ItemType> ActiveItemTypes { get; set; } = [];
    public List<ItemType> DeletedItemTypes { get; set; } = [];

    // ItemTags
    public List<ItemTag> ActiveItemTags { get; set; } = [];
    public List<ItemTag> DeletedItemTags { get; set; } = [];

    public async Task OnGetAsync()
    {
        // Load MenuItems
        ActiveMenuItems = await _menuService.GetMenuItemsAsync(showUnavailable: true, isRestore: false);
        DeletedMenuItems = await _menuService.GetMenuItemsAsync(showUnavailable: true, isRestore: true);

        // Load Sets
        ActiveSets = await _menuService.GetMenuItemsAsync(showUnavailable: true, getSets: true, isRestore: false);
        DeletedSets = await _menuService.GetMenuItemsAsync(showUnavailable: true, getSets: true, isRestore: true);

        // Load Bundles
        ActiveBundles = await _bundleService.GetBundleItemsAsync(showUnavailable: true, isRestore: false);
        DeletedBundles = await _bundleService.GetBundleItemsAsync(showUnavailable: true, isRestore: true);

        // Load ItemTypes
        ActiveItemTypes = await _itemTypeService.GetItemTypesAsync(isRestore: false);
        DeletedItemTypes = await _itemTypeService.GetItemTypesAsync(isRestore: true);

        // Load ItemTags
        ActiveItemTags = await _itemTagService.GetItemTagsAsync(isRestore: false);
        DeletedItemTags = await _itemTagService.GetItemTagsAsync(isRestore: true);
    }
}