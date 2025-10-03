using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Menu.Tags;

public class Index : PageModel
{
    private readonly ItemTagService _itemTagService;

    public Index(ItemTagService itemTagService)
    {
        _itemTagService = itemTagService;
    }

    public List<ItemTag> ItemTags { get; set; }

    public async Task<IActionResult> OnGet()
    {
        ItemTags = await _itemTagService.GetItemTagsAsync();
        return Page();
    }
}