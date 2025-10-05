using AlwahaLibrary.Authentication;
using AlwahaLibrary.Models;
using AlwahaManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages.Audit;

[Authorize(Roles = SystemRoles.Admin)]
public class Trail : PageModel
{
    private readonly AuditService _auditService;
    private readonly UserManager<IdentityUser> _userManager;

    public Trail(AuditService auditService, UserManager<IdentityUser> userManager)
    {
        _auditService = auditService;
        _userManager = userManager;
    }

    public List<AuditEntry> AuditEntries { get; set; } = [];
    public bool IsUser { get; set; }
    public string EntityName { get; set; } = "";
    public string TableName { get; set; } = "";
    public string Id { get; set; } = "";
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 20;
    public int TotalEntries { get; set; }

    public async Task<IActionResult> OnGetAsync(string id, [FromQuery] bool isUser = false, [FromQuery] int page = 1)
    {
        Id = id;
        IsUser = isUser;
        CurrentPage = page < 1 ? 1 : page;

        if (IsUser)
        {
            // User audit trail
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            EntityName = user.UserName ?? user.Email ?? "Unknown User";
            var (entries, total) = await _auditService.GetAuditEntriesForUserAsync(id, CurrentPage, PageSize);
            AuditEntries = entries;
            TotalEntries = total;
        }
        else
        {
            // Entity audit trail - id should be in format "tableName:entityId"
            var parts = id.Split(':');
            if (parts.Length != 2) return BadRequest("Invalid entity identifier");

            TableName = parts[0];
            var entityId = parts[1];

            var name = await _auditService.GetEntityNameAsync(TableName, entityId);
            EntityName = name ?? "Unknown Entity";

            var (entries, total) = await _auditService.GetAuditEntriesForEntityAsync(TableName, entityId, CurrentPage, PageSize);
            AuditEntries = entries;
            TotalEntries = total;
        }

        TotalPages = (int)Math.Ceiling(TotalEntries / (double)PageSize);

        return Page();
    }
}