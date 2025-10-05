using Microsoft.AspNetCore.Identity;

namespace AlwahaManagement.Models;

public class UserModel
{
    public IdentityUser User { get; set; }
    public List<string> Roles { get; set; }
}