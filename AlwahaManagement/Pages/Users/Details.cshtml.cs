using AlwahaLibrary.Authentication;
using AlwahaManagement.Models;
using AlwahaManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Users;

[Authorize(Roles = SystemRoles.Admin)]
public class Details : PageModel
{
    private readonly AdminService _adminService;

    public Details(AdminService adminService)
    {
        _adminService = adminService;
    }

    public UserModel UserModel { get; set; } = null!;

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? Password { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var user = await _adminService.GetUser(id);
        if (user == null)
        {
            return NotFound();
        }

        UserModel = user;
        return Page();
    }

    public async Task<IActionResult> OnPostAddRoleAsync(string id, string role)
    {
        var success = await _adminService.TryAddRole(id, role);
        if (success)
        {
            TempData["Message"] = $"Role '{role}' added successfully.";
        }
        else
        {
            TempData["Message"] = $"Failed to add role '{role}'.";
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemoveRoleAsync(string id, string role)
    {
        var success = await _adminService.TryRemoveRole(id, role);
        if (success)
        {
            TempData["Message"] = $"Role '{role}' removed successfully.";
        }
        else
        {
            TempData["Message"] = $"Failed to remove role '{role}'.";
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(string id)
    {
        var newPassword = await _adminService.TryResetPassword(id);
        if (!string.IsNullOrEmpty(newPassword))
        {
            TempData["Password"] = newPassword;
            TempData["Message"] = "Password reset successfully!";
        }
        else
        {
            TempData["Message"] = "Failed to reset password.";
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var success = await _adminService.TryDeleteUser(id);
        if (success)
        {
            TempData["Message"] = "User deleted successfully.";
            return RedirectToPage("./Index");
        }

        TempData["Message"] = "Failed to delete user.";
        return RedirectToPage(new { id });
    }
}