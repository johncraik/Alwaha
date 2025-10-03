using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Menu.ItemTypes;

public class Index : PageModel
{
    private readonly ItemTypeService _itemTypeService;

    public Index(ItemTypeService itemTypeService)
    {
        _itemTypeService = itemTypeService;
    }
    
    public List<ItemType> ItemTypes { get; set; }
    
    public async Task<IActionResult> OnGet()
    {
        ItemTypes = await _itemTypeService.GetItemTypesAsync();
        return Page();
    }
}