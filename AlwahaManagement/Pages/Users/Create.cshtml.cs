using AlwahaLibrary.Authentication;
using AlwahaManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AlwahaManagement.Pages.Users;

[Authorize(Roles = SystemRoles.Admin)]
public class Create : PageModel
{
    private readonly AdminService _adminService;

    public Create(AdminService adminService)
    {
        _adminService = adminService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var (success, userId, password) = await _adminService.TryCreateUserAsync(Input.Username, Input.Email);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to create user. Username or email may already exist.");
            return Page();
        }

        TempData["Password"] = password;
        TempData["Message"] = "User created successfully!";

        return RedirectToPage("./Details", new { id = userId });
    }
}