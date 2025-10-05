using AlwahaLibrary.Authentication;
using AlwahaManagement.Models;
using AlwahaManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Audit;

[Authorize(Roles = SystemRoles.Admin)]
public class User : PageModel
{
    private readonly AdminService _adminService;

    public User(AdminService adminService)
    {
        _adminService = adminService;
    }

    public List<UserModel> Users { get; set; } = [];

    public async Task OnGetAsync()
    {
        Users = await _adminService.GetUsers();
    }
}