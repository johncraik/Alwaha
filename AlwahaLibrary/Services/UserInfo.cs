
using AlwahaLibrary.Authentication;

namespace AlwahaLibrary.Services;

public class UserInfo
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; }

    public bool IsSetup { get; set; }

    public bool IsInRole(string role)
        => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public bool CanCreate()
        => IsInRole(SystemRoles.CreatePermission) || IsInRole(SystemRoles.Admin);
    
    public bool CanEdit()
        => IsInRole(SystemRoles.EditPermission) || IsInRole(SystemRoles.Admin);
    
    public bool CanDelete()
        => IsInRole(SystemRoles.DeletePermission) || IsInRole(SystemRoles.Admin);
}