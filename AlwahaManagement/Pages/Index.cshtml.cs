using System.ComponentModel.DataAnnotations;
using AlwahaManagement.Models;
using AlwahaManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly DashboardService _dashboardService;

    public IndexModel(ILogger<IndexModel> logger,
        DashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }
    
    public MenuStat MenuItemsStat { get; set; }
    public MenuStat MenuSetStats { get; set; }
    public MenuStat BundleItemsStat { get; set; }
    public MenuStat ItemTypesStat { get; set; }
    public MenuStat ItemTagsStat { get; set; }
    public UserStat UserStats { get; set; }
    
    public async Task OnGet()
    {
        MenuItemsStat = await _dashboardService.GetMenuItemStats();
        MenuSetStats = await _dashboardService.GetMenuSetStats();
        BundleItemsStat = await _dashboardService.GetMenuBundleStats();
        ItemTypesStat = await _dashboardService.GetItemTypeStats();
        ItemTagsStat = await _dashboardService.GetItemTagStats();
        UserStats = await _dashboardService.GetUserStats();
    }
}