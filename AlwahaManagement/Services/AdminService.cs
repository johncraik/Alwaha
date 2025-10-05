using AlwahaLibrary.Authentication;
using AlwahaLibrary.Services;
using AlwahaManagement.Data;
using AlwahaManagement.Helpers;
using AlwahaManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlwahaManagement.Services;

public class AdminService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly UserInfo _userInfo;
    private readonly SettingsService _settingsService;

    public AdminService(ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        UserInfo userInfo,
        SettingsService settingsService)
    {
        _context = context;
        _userManager = userManager;
        _userInfo = userInfo;
        _settingsService = settingsService;
    }

    private bool Authorised()
        => _userInfo.IsInRole(SystemRoles.Admin);

    public async Task<List<UserModel>> GetUsers()
    {
        if (!Authorised()) return [];
        
        var users = await _context.Users.ToListAsync();
        var models = new List<UserModel>();
        foreach (var user in users)
        {
            var roleIds = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .ToListAsync();
            var roles = await _context.Roles
                .Where(r => roleIds.Contains(r.Id) && !string.IsNullOrEmpty(r.Name))
                .Select(r => r.Name!)
                .ToListAsync();
            models.Add(new UserModel
                {
                    User = user,
                    Roles = roles
                }
            );
        }
        
        return models.OrderBy(u => u.User.UserName).ToList();
    }

    public async Task<UserModel?> GetUser(string id)
    {
        if (!Authorised()) return null;
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if(user == null) return null;
        
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync();
        var roles = await _context.Roles
            .Where(r => roleIds.Contains(r.Id) && !string.IsNullOrEmpty(r.Name))
            .Select(r => r.Name!)
            .ToListAsync();
        return new UserModel
        {
            User = user,
            Roles = roles
        };
    }

    public async Task<(bool Success, string UserId, string Password)> TryCreateUserAsync(string username, string email)
    {
        if (!Authorised()) return (false, string.Empty, string.Empty);

        var existingUser = await _userManager.FindByNameAsync(username);
        if (existingUser != null) return (false, string.Empty, string.Empty);
        existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null) return (false, string.Empty, string.Empty);

        // Get settings
        var autoConfirmEmail = await _settingsService.GetSettingAsync<bool>("AutoConfirmEmail");
        var generatedPasswordLength = await _settingsService.GetSettingAsync<int>("GeneratedPasswordLength");

        var user = new IdentityUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = autoConfirmEmail,
            TwoFactorEnabled = false
        };

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded) return (false, string.Empty, string.Empty);

        var password = PasswordHelper.GenerateStrongPassword(generatedPasswordLength);
        result = await _userManager.AddPasswordAsync(user, password);
        return !result.Succeeded ? (false, string.Empty, string.Empty) : (true, user.Id , password);
    }

    public async Task<bool> TryAddRole(string userId, string roleName)
    {
        if (!Authorised()) return false;
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;
        
        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }
    
    public async Task<bool> TryRemoveRole(string userId, string roleName)
    {
        if (!Authorised()) return false;
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;
        
        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        return result.Succeeded;
    }
    
    public async Task<string> TryResetPassword(string userId)
    {
        if (!Authorised()) return string.Empty;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return string.Empty;

        // Get setting for generated password length
        var generatedPasswordLength = await _settingsService.GetSettingAsync<int>("GeneratedPasswordLength");
        var newPassword = PasswordHelper.GenerateStrongPassword(generatedPasswordLength);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            return string.Empty;
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.UpdateSecurityStampAsync(user);

        return newPassword;
    }
    
    public async Task<bool> TryDeleteUser(string userId)
    {
        if (!Authorised()) return false;
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}