using System.ComponentModel.DataAnnotations;
using AlwahaLibrary.Helpers;
using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Menu.ItemTypes;

public class Edit : PageModel
{
    private readonly ItemTypeService _itemTypeService;
    public bool Adding { get; set; }

    public Edit(ItemTypeService itemTypeService)
    {
        _itemTypeService = itemTypeService;
    }

    public class ItemTypeInputModel
    {
        [Required]
        public string Name { get; set; }
        public string? Colour { get; set; }

        public ItemTypeInputModel()
        {
        }

        public ItemTypeInputModel(ItemType type)
        {
            Name = type.Name;
            //Colour = type.Colour;
        }

        public void Fill(ItemType type)
        {
            type.Name = Name;
            
            //TODO not required yet - built into backend in-case needed
            Colour = null; 
            //type.Colour = Colour;
        }
    }
    
    [BindProperty]
    public ItemTypeInputModel Input { get; set; }

    public void SetAdding(string? id)
    {
        Adding = string.IsNullOrEmpty(id);
    }
    
    public async Task<IActionResult> OnGet(string? id)
    {
        SetAdding(id);
        if (Adding)
        {
            Input = new ItemTypeInputModel();
            return Page();
        }
        
        var type = await _itemTypeService.GetItemTypeAsync(id!);
        if(type == null) return NotFound();
        
        Input = new ItemTypeInputModel(type);
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
            var type = new ItemType();
            Input.Fill(type);
            res = await _itemTypeService.TryCreateItemTypeAsync(type, modelState);
        }
        else
        {
            var type = await _itemTypeService.GetItemTypeAsync(id!);
            if (type == null) return NotFound();
            
            Input.Fill(type);
            res = await _itemTypeService.TryUpdateItemTypeAsync(type, modelState);
        }
        
        return res ? RedirectToPage("./Index") : Page();
    }
}