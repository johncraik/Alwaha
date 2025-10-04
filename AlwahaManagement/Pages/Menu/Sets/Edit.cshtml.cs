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
    public bool Adding { get; set; }

    public Edit(MenuService menuService, ItemTypeService itemTypeService)
    {
        _menuService = menuService;
        _itemTypeService = itemTypeService;
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
            SelectedMenuItems = await _menuService.GetMenuItemIdsInSetAsync(set.ItemId)
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
        if (Adding)
        {
            var set = new MenuItem();
            Input.Fill(set);
            res = await _menuService.TryCreateSetAsync(set, Input.SelectedMenuItems, modelState);
        }
        else
        {
            var set = await _menuService.GetMenuItemAsync(id!);
            if (set == null) return NotFound();

            Input.Fill(set);
            res = await _menuService.TryUpdateSetAsync(set, Input.SelectedMenuItems, modelState);
        }

        return res ? RedirectToPage("./Index") : Page();
    }
}