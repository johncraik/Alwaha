using AlwahaLibrary.Helpers;
using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlwahaManagement.Pages.Menu.Sets;

public class Edit : PageModel
{
    private readonly MenuService _menuService;
    private readonly ItemTypeService _itemTypeService;
    private readonly ItemTagService _itemTagService;
    public bool Adding { get; set; }

    public Edit(MenuService menuService, ItemTypeService itemTypeService, ItemTagService itemTagService)
    {
        _menuService = menuService;
        _itemTypeService = itemTypeService;
        _itemTagService = itemTagService;
    }
    
    public class SetInputModel : Menu.Edit.MenuItemInputModel
    {
        public string? Colour { get; set; }
        public List<string> SelectedMenuItems { get; set; } = [];

        public SetInputModel()
        {
        }

        public SetInputModel(MenuItem item) 
            : base(item)
        {
            Colour = item.Colour;
        }
        
        public override void Fill(MenuItem item, bool isSet = false)
        {
            base.Fill(item, true);
            item.Colour = Colour;
        }
    }

    [BindProperty]
    public SetInputModel Input { get; set; }
    public List<IGrouping<ItemType, MenuItem>> MenuItems { get; set; }
    public List<SelectListItem> ItemTypes { get; set; }
    public List<ItemTag> ItemTags { get; set; }

    public void SetupAdding(string? id)
    {
        Adding = string.IsNullOrEmpty(id);
    }

    public async Task SetupPage()
    {
        MenuItems = await _menuService.GetMenuItemsAsync();
        ItemTypes = (await _itemTypeService.GetItemTypesAsync())
            .Select(t => new SelectListItem
            {
                Text = t.Name,
                Value = t.ItemTypeId
            })
            .ToList();
        ItemTags = await _itemTagService.GetItemTagsAsync();
    }

    public async Task<IActionResult> OnGet(string? id)
    {
        SetupAdding(id);
        await SetupPage();
        
        if (Adding)
        {
            Input = new SetInputModel();
            return Page();
        }
        
        var set = await _menuService.GetMenuItemAsync(id!);
        if(set == null) return NotFound();

        Input = new SetInputModel(set)
        {
            SelectedMenuItems = await _menuService.GetMenuItemIdsInSetAsync(set.ItemId),
            SelectedTagIds = await _itemTagService.GetItemTagIdsAsync(id!)
        };
        return Page();
    }
    
    
    public async Task<IActionResult> OnPost(string? id)
    {
        SetupAdding(id);
        await SetupPage();
        if (!ModelState.IsValid) return Page();

        var modelState = new ModelStateWrapper(ModelState);
        bool res;
        string setId;
        if (Adding)
        {
            var set = new MenuItem();
            Input.Fill(set);
            res = await _menuService.TryCreateSetAsync(set, Input.SelectedMenuItems, modelState);
            setId = set.ItemId;
        }
        else
        {
            var set = await _menuService.GetMenuItemAsync(id!);
            if (set == null) return NotFound();

            Input.Fill(set);
            res = await _menuService.TryUpdateSetAsync(set, Input.SelectedMenuItems, modelState);
            setId = set.ItemId;
        }

        if (!res) return Page();

        res = await _itemTagService.TryModifyItemTagsAsync(setId, Input.SelectedTagIds);
        if(res) return RedirectToPage("./Index");

        modelState.AddModelError("SelectedTagIds", "Failed to update set tags");
        return Page();
    }
}