using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Menu.Tags;

public class Index : PageModel
{
    private readonly ItemTagService _itemTagService;
    private readonly UserInfo _userInfo;

    public Index(ItemTagService itemTagService, UserInfo userInfo)
    {
        _itemTagService = itemTagService;
        _userInfo = userInfo;
    }

    public List<ItemTag> ItemTags { get; set; }
    public bool IsRestore { get; set; }

    public async Task<IActionResult> OnGet(bool isRestore = false)
    {
        IsRestore = isRestore && _userInfo.CanRestore();
        ItemTags = await _itemTagService.GetItemTagsAsync(isRestore);
        return Page();
    }
}