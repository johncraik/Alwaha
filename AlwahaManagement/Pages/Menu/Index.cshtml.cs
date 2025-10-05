using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Menu;

public class Index : PageModel
{
    private readonly MenuService _menuService;
    private readonly UserInfo _userInfo;

    public Index(MenuService menuService, UserInfo userInfo)
    {
        _menuService = menuService;
        _userInfo = userInfo;
    }

    public List<IGrouping<ItemType, MenuItem>> MenuItems { get; set; }
    public string Search { get; set; }
    public bool IsRestore { get; set; }

    public async Task OnGet(string search = "", bool isRestore = false)
    {
        Search = search;
        IsRestore = isRestore && _userInfo.CanRestore();
        MenuItems = await _menuService.GetMenuItemsAsync(search: search, showUnavailable: true, isRestore: isRestore);
    }
}