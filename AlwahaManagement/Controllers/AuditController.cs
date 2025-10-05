using AlwahaManagement.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AlwahaManagement.Controllers;

public class AuditController : Controller
{
    private readonly AuditService _auditService;
    private readonly UserManager<IdentityUser> _userManager;

    public AuditController(AuditService auditService, UserManager<IdentityUser> userManager)
    {
        _auditService = auditService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<string> GetEntityName(string tableName, string entityId)
    {
        var name = await _auditService.GetEntityNameAsync(tableName, entityId);
        return name ?? "Unknown";
    }

    [HttpGet]
    public async Task<string> GetUserName(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.UserName ?? user?.Email ?? "Unknown";
    }
}