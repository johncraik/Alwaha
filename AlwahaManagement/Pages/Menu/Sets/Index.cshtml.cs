using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Menu.Sets;

public class Index : PageModel
{
    private readonly MenuService _menuService;

    public Index(MenuService menuService)
    {
        _menuService = menuService;
    }

    public List<IGrouping<ItemType, MenuItem>> MenuSets { get; set; }

    public async Task OnGet()
    {
        MenuSets = await _menuService.GetMenuItemsAsync(showUnavailable: true, getSets: true);
    }
}