using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AlwahaLibrary.Helpers;
using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlwahaManagement.Pages.Menu;

public class Edit : PageModel
{
    private readonly MenuService _menuService;
    private readonly ItemTypeService _itemTypeService;

    public bool Adding { get; set; }

    public Edit(MenuService menuService,
        ItemTypeService itemTypeService)
    {
        _menuService = menuService;
        _itemTypeService = itemTypeService;
    }

    public class MenuItemInputModel
    {
        [Required]
        [DisplayName("Item Type")]
        public string ItemTypeId { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public double Price { get; set; }
        public string? ImagePath { get; set; }
        public bool IsAvailable { get; set; } = true;

        public MenuItemInputModel()
        {
        }

        public MenuItemInputModel(MenuItem item)
        {
            ItemTypeId = item.ItemTypeId;
            Name = item.Name;
            Description = item.Description;
            Price = item.Price;
            ImagePath = item.ImagePath;
            IsAvailable = item.IsAvailable;
        }

        public void Fill(MenuItem item)
        {
            item.ItemTypeId = ItemTypeId;
            item.Name = Name;
            item.Description = Description;
            item.Price = Price;
            item.ImagePath = ImagePath;
            item.IsAvailable = IsAvailable;
            item.IsSet = false;
        }
    }
    
    [BindProperty]
    public MenuItemInputModel Input { get; set; }
    public List<SelectListItem> ItemTypes { get; set; }

    public async Task SetupPage()
    {
        ItemTypes = (await _itemTypeService.GetItemTypesAsync())
            .Select(t => new SelectListItem
            {
                Text = t.Name,
                Value = t.ItemTypeId
            })
            .ToList();
    }

    public void SetupAdding(string? id)
    {
        Adding = string.IsNullOrEmpty(id);
    }
    
    public async Task<IActionResult> OnGet(string? id)
    {
        await SetupPage();
        SetupAdding(id);

        if (Adding)
        {
            Input = new MenuItemInputModel();
            return Page();
        }

        var item = await _menuService.GetMenuItemAsync(id!);
        if(item == null) return NotFound();
        
        Input = new MenuItemInputModel(item);
        return Page();
    }

    public async Task<IActionResult> OnPost(string? id)
    {
        await SetupPage();
        SetupAdding(id);
        if (!ModelState.IsValid) return Page();

        var msw = new ModelStateWrapper(ModelState);
        bool res;
        if (Adding)
        {
            var item = new MenuItem();
            Input.Fill(item);
            res = await _menuService.TryCreateMenuItemAsync(item, msw);
        }
        else
        {
            var item = await _menuService.GetMenuItemAsync(id!);
            if (item == null) return NotFound();
            
            Input.Fill(item);
            res = await _menuService.TryUpdateMenuItemAsync(item, msw);
        }
        
        return res ? RedirectToPage("./Index") : Page();
    }
}