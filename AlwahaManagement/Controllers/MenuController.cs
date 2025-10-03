using System.ComponentModel.Design;
using AlwahaLibrary.Data;
using AlwahaLibrary.Helpers;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlwahaManagement.Controllers;

public class MenuController : Controller
{
    private readonly MenuService _menuService;
    private readonly ItemTypeService _itemTypeService;
    private readonly ItemTagService _itemTagService;

    public MenuController(MenuService menuService,
        ItemTypeService itemTypeService,
        ItemTagService itemTagService)
    {
        _menuService = menuService;
        _itemTypeService = itemTypeService;
        _itemTagService = itemTagService;
    }


    [HttpGet]
    public async Task<IActionResult> GetMenuItemsList()
    {
        var items = await _menuService.GetMenuItemsAsync(showUnavailable: true);
        return PartialView("Menu/_MenuItemTable", items);
    }
    
    
    [HttpPost]
    public async Task<IActionResult> ReorderTypes([FromBody] ReorderRequest request)
    {
        if(request.Ids == null! || request.Ids.Count == 0) return BadRequest();
        
        var items = await _itemTypeService.GetItemTypesAsync();
        
        for (var i = 0; i < request.Ids.Count; i++)
        {
            var item = items.FirstOrDefault(it => it.ItemTypeId == request.Ids[i]);;
            if(item == null) continue;
            
            item.Order = i + 1;
            await _itemTypeService.TryUpdateItemTypeAsync(item, new ModelStateWrapper(ModelState));
        }
        
        return Ok();
    }
    
    public class ReorderRequest
    {
        public List<string> Ids { get; set; } = new();
    }
    
    
    [HttpPost]
    public async Task<bool> DeleteItem(string id)
    {
        var item = await _menuService.GetMenuItemAsync(id);
        if(item == null) return false;
        
        return await _menuService.TryDeleteMenuItemAsync(item);
    }

    [HttpPost]
    public async Task<bool> DeleteType(string id)
    {
        var type = await _itemTypeService.GetItemTypeAsync(id);
        if(type == null) return false;
        
        return await _itemTypeService.TryDeleteItemTypeAsync(type);
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

    [HttpGet]
    public async Task<IActionResult> GetItemTagsList()
    {
        var tags = await _itemTagService.GetItemTagsAsync();
        return PartialView("Menu/_ItemTagList", tags);
    }

    [HttpPost]
    public async Task<bool> DeleteTag(string id)
    {
        var tag = await _itemTagService.GetItemTagAsync(id);
        if(tag == null) return false;

        return await _itemTagService.TryDeleteItemTagAsync(tag);
    }
}