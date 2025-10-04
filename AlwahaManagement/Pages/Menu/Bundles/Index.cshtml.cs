using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Menu.Bundles;

public class Index : PageModel
{
    private readonly BundleService _bundleService;

    public Index(BundleService bundleService)
    {
        _bundleService = bundleService;
    }

    public List<IGrouping<ItemType, BundleItem>> BundleItems { get; set; }
    public string Search { get; set; }

    public async Task OnGet(string search = "")
    {
        Search = search;
        BundleItems = await _bundleService.GetBundleItemsAsync(search: search, showUnavailable: true);
    }
}