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
    private readonly ItemTypeService _typeService;

    public bool Adding { get; set; }

    public Edit(BundleService bundleService, ItemTypeService typeService)
    {
        _bundleService = bundleService;
        _typeService = typeService;
    }

    public class BundleItemInputModel
    {
        [Required]
        [DisplayName("Menu Item")]
        public string TypeId { get; set; }

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
            TypeId = bundle.ItemTypeId;
            Quantity = bundle.Quantity;
            Price = bundle.Price;
        }

        public void Fill(BundleItem bundle)
        {
            bundle.ItemTypeId = TypeId;
            bundle.Quantity = Quantity;
            bundle.Price = Price;
        }
    }

    [BindProperty]
    public BundleItemInputModel Input { get; set; }
    public List<SelectListItem> ItemTypes { get; set; }

    public void SetupAdding(string? id)
    {
        Adding = string.IsNullOrEmpty(id);
    }

    public async Task SetupPage()
    {
        var itemTypes = await _typeService.GetItemTypesAsync();
        ItemTypes = itemTypes.Select(t => new SelectListItem
            {
                Text = t.Name,
                Value = t.ItemTypeId
            })
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