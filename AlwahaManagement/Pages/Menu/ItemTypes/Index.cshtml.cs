using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Menu.ItemTypes;

public class Index : PageModel
{
    private readonly ItemTypeService _itemTypeService;
    private readonly UserInfo _userInfo;

    public Index(ItemTypeService itemTypeService, UserInfo userInfo)
    {
        _itemTypeService = itemTypeService;
        _userInfo = userInfo;
    }

    public List<ItemType> ItemTypes { get; set; }
    public bool IsRestore { get; set; }

    public async Task<IActionResult> OnGet(bool isRestore = false)
    {
        IsRestore = isRestore && _userInfo.CanRestore();
        ItemTypes = await _itemTypeService.GetItemTypesAsync(isRestore);
        return Page();
    }
}