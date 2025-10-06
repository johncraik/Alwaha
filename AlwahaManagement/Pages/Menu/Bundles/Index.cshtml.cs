using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Menu.Bundles;

public class Index : PageModel
{
    private readonly BundleService _bundleService;
    private readonly UserInfo _userInfo;

    public Index(BundleService bundleService, UserInfo userInfo)
    {
        _bundleService = bundleService;
        _userInfo = userInfo;
    }

    public List<IGrouping<ItemType, BundleItem>> BundleItems { get; set; }
    public string Search { get; set; }
    public bool IsRestore { get; set; }

    public async Task OnGet(string search = "", bool isRestore = false)
    {
        Search = search;
        IsRestore = isRestore && _userInfo.CanRestore();
        BundleItems = await _bundleService.GetBundleItemsAsync(search: search, isRestore: isRestore);
    }
}