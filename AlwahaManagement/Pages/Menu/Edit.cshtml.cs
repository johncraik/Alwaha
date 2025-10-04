using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AlwahaLibrary.Helpers;
using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;
using Newtonsoft.Json.Converters;

namespace AlwahaManagement.Pages.Menu;

public class Edit : PageModel
{
    private readonly MenuService _menuService;
    private readonly ItemTypeService _itemTypeService;
    private readonly ItemTagService _itemTagService;

    public bool Adding { get; set; }

    public Edit(MenuService menuService,
        ItemTypeService itemTypeService,
        ItemTagService itemTagService)
    {
        _menuService = menuService;
        _itemTypeService = itemTypeService;
        _itemTagService = itemTagService;
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
        [DisplayName("Available?")]
        public bool IsAvailable { get; set; } = true;
        public List<string> SelectedTagIds { get; set; } = [];

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

        public virtual void Fill(MenuItem item, bool isSet = false)
        {
            item.ItemTypeId = ItemTypeId;
            item.Name = Name;
            item.Description = Description;
            item.Price = Price;
            item.ImagePath = ImagePath;
            item.IsAvailable = IsAvailable;
            item.IsSet = isSet;
        }
    }
    
    [BindProperty]
    public MenuItemInputModel Input { get; set; }
    public List<SelectListItem> ItemTypes { get; set; }
    public List<ItemTag> ItemTags { get; set; }

    public async Task SetupPage()
    {
        ItemTypes = (await _itemTypeService.GetItemTypesAsync())
            .Select(t => new SelectListItem
            {
                Text = t.Name,
                Value = t.ItemTypeId
            })
            .ToList();
        ItemTags = await _itemTagService.GetItemTagsAsync();
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

        Input = new MenuItemInputModel(item)
        {
            SelectedTagIds = await _itemTagService.GetItemTagIdsAsync(id!)
        };
        return Page();
    }

    public async Task<IActionResult> OnPost(string? id)
    {
        await SetupPage();
        SetupAdding(id);
        if (!ModelState.IsValid) return Page();

        var msw = new ModelStateWrapper(ModelState);
        bool res;
        string itemId;
        if (Adding)
        {
            var item = new MenuItem();
            Input.Fill(item);
            res = await _menuService.TryCreateMenuItemAsync(item, msw);
            itemId = item.ItemId;
        }
        else
        {
            var item = await _menuService.GetMenuItemAsync(id!);
            if (item == null) return NotFound();

            Input.Fill(item);
            res = await _menuService.TryUpdateMenuItemAsync(item, msw);
            itemId = item.ItemId;
        }

        if (!res) return Page();
        
        res = await _itemTagService.TryModifyItemTagsAsync(itemId, Input.SelectedTagIds);
        if(res) return RedirectToPage("./Index");
        
        msw.AddModelError("SelectedTagIds", "Failed to update item tags");
        return Page();
    }
}