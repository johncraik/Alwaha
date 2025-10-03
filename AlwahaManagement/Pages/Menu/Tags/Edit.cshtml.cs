using System.ComponentModel.DataAnnotations;
using AlwahaLibrary.Helpers;
using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Menu.Tags;

public class Edit : PageModel
{
    private readonly ItemTagService _itemTagService;
    public bool Adding { get; set; }

    public Edit(ItemTagService itemTagService)
    {
        _itemTagService = itemTagService;
    }

    public class ItemTagInputModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Colour { get; set; }

        [Required]
        public string Icon { get; set; }

        public ItemTagInputModel()
        {
        }

        public ItemTagInputModel(ItemTag tag)
        {
            Name = tag.Name;
            Colour = tag.Colour;
            Icon = tag.Icon;
        }

        public void Fill(ItemTag tag)
        {
            tag.Name = Name;
            tag.Colour = Colour;
            tag.Icon = Icon;
        }
    }

    [BindProperty]
    public ItemTagInputModel Input { get; set; }

    public void SetAdding(string? id)
    {
        Adding = string.IsNullOrEmpty(id);
    }

    public async Task<IActionResult> OnGet(string? id)
    {
        SetAdding(id);
        if (Adding)
        {
            Input = new ItemTagInputModel();
            return Page();
        }

        var tag = await _itemTagService.GetItemTagAsync(id!);
        if(tag == null) return NotFound();

        Input = new ItemTagInputModel(tag);
        return Page();
    }

    public async Task<IActionResult> OnPost(string? id)
    {
        SetAdding(id);
        if (!ModelState.IsValid) return Page();

        var modelState = new ModelStateWrapper(ModelState);
        bool res;
        if (Adding)
        {
            var tag = new ItemTag();
            Input.Fill(tag);
            res = await _itemTagService.TryCreateItemTagAsync(tag, modelState);
        }
        else
        {
            var tag = await _itemTagService.GetItemTagAsync(id!);
            if (tag == null) return NotFound();

            Input.Fill(tag);
            res = await _itemTagService.TryUpdateItemTagAsync(tag, modelState);
        }

        return res ? RedirectToPage("./Index") : Page();
    }
}