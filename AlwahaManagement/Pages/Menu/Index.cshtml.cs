using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Menu;

public class Index : PageModel
{
    private readonly MenuService _menuService;

    public Index(MenuService menuService)
    {
        _menuService = menuService;
    }
    
    public List<IGrouping<ItemType, MenuItem>> MenuItems { get; set; }
    public string Search { get; set; }

    public async Task OnGet(string search = "")
    {
        Search = search;
        MenuItems = await _menuService.GetMenuItemsAsync(search: search, showUnavailable: true);
    }
}