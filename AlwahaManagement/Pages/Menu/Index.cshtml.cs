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
    
    public List<MenuItem> MenuItems { get; set; }
    
    public async Task OnGet()
    {
        MenuItems = await _menuService.GetMenuItemsAsync();
    }
}