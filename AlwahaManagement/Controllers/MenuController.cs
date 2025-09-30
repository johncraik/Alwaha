using AlwahaLibrary.Data;
using AlwahaLibrary.Helpers;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlwahaManagement.Controllers;

public class MenuController : Controller
{
    private readonly MenuService _menuService;

    public MenuController(MenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpPost]
    public async Task<bool> DeleteItem(string id)
    {
        var item = await _menuService.GetMenuItemAsync(id);
        if(item == null) return false;
        
        return await _menuService.TryDeleteMenuItemAsync(item);
    }

    [HttpPost]
    public async Task<bool> ToggleAvailable(string id, bool isAvailable)
    {
        var item = await _menuService.GetMenuItemAsync(id);
        if (item == null) return false;

        var modelState = new ModelStateWrapper(ModelState);
        item.IsAvailable = isAvailable;
        return await _menuService.TryUpdateMenuItemAsync(item, modelState);
    }
}