using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AlwahaLibrary.Authentication;
using AlwahaLibrary.Helpers;
using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlwahaManagement.Pages.Menu.Bundles;

[Authorize(Roles = $"{SystemRoles.Admin}, {SystemRoles.CreatePermission}, {SystemRoles.EditPermission}")]
public class Edit : PageModel
{
    private readonly BundleService _bundleService;
    private readonly MenuService _menuService;

    public bool Adding { get; set; }

    public Edit(BundleService bundleService, MenuService menuService)
    {
        _bundleService = bundleService;
        _menuService = menuService;
    }

    public class BundleItemInputModel
    {
        [Required]
        [DisplayName("Menu Item")]
        public string ItemId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public double Price { get; set; }

        public BundleItemInputModel()
        {
        }

        public BundleItemInputModel(BundleItem bundle)
        {
            ItemId = bundle.ItemId;
            Quantity = bundle.Quantity;
            Price = bundle.Price;
        }

        public void Fill(BundleItem bundle)
        {
            bundle.ItemId = ItemId;
            bundle.Quantity = Quantity;
            bundle.Price = Price;
        }
    }

    [BindProperty]
    public BundleItemInputModel Input { get; set; }
    public List<SelectListItem> MenuItems { get; set; }

    public void SetupAdding(string? id)
    {
        Adding = string.IsNullOrEmpty(id);
    }

    public async Task SetupPage()
    {
        var menuItemGroups = await _menuService.GetMenuItemsAsync();
        MenuItems = menuItemGroups
            .SelectMany(g => g.Select(item => new SelectListItem
            {
                Text = $"{g.Key.Name} - {item.Name} - {item.Price:C}",
                Value = item.ItemId
            }))
            .ToList();
    }

    public async Task<IActionResult> OnGet(string? id)
    {
        SetupAdding(id);
        await SetupPage();

        if (Adding)
        {
            Input = new BundleItemInputModel();
            return Page();
        }

        var bundle = await _bundleService.GetBundleItemAsync(id!);
        if (bundle == null) return NotFound();

        Input = new BundleItemInputModel(bundle);
        return Page();
    }

    public async Task<IActionResult> OnPost(string? id)
    {
        SetupAdding(id);
        await SetupPage();
        if (!ModelState.IsValid) return Page();

        var modelState = new ModelStateWrapper(ModelState);
        bool res;
        if (Adding)
        {
            var bundle = new BundleItem();
            Input.Fill(bundle);
            res = await _bundleService.TryCreateBundleItemAsync(bundle, modelState);
        }
        else
        {
            var bundle = await _bundleService.GetBundleItemAsync(id!);
            if (bundle == null) return NotFound();

            Input.Fill(bundle);
            res = await _bundleService.TryUpdateBundleItemAsync(bundle, modelState);
        }

        return res ? RedirectToPage("./Index") : Page();
    }
}