using AlwahaLibrary.Authentication;
using AlwahaLibrary.Data;
using AlwahaLibrary.Services;
using AlwahaManagement.Data;
using AlwahaManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace AlwahaManagement.Services;

public class DashboardService
{
    private readonly AlwahaDbContext _context;
    private readonly ApplicationDbContext _authContext;
    private readonly UserInfo _userInfo;

    public DashboardService(AlwahaDbContext context,
        ApplicationDbContext authContext,
        UserInfo userInfo)
    {
        _context = context;
        _authContext = authContext;
        _userInfo = userInfo;
    }

    public async Task<MenuStat> GetMenuItemStats()
    {
        var items = await _context.MenuItems
            .Where(i => !i.IsSet && !i.IsDeleted)
            .ToListAsync();
        return new MenuStat
        {
            Available = (uint)items.Count(i => i.IsAvailable),
            Unavailable = (uint)items.Count(i => !i.IsAvailable)
        };
    }

    public async Task<MenuStat> GetMenuSetStats()
    {
        var sets = await _context.MenuItems
            .Where(s => s.IsSet && !s.IsDeleted)
            .ToListAsync();
        return new MenuStat
        {
            Available = (uint)sets.Count(s => s.IsAvailable),
            Unavailable = (uint)sets.Count(s => !s.IsAvailable)
        };
    }

    public async Task<MenuStat> GetMenuBundleStats()
    {
        var bundles = await _context.BundleItems
            .Include(bundleItem => bundleItem.MenuItem)
            .Where(b => !b.IsDeleted)
            .ToListAsync();
        return new MenuStat
        {
            Available = (uint)bundles.Count(b => b.MenuItem.IsAvailable),
            Unavailable = (uint)bundles.Count(b => !b.MenuItem.IsAvailable)
        };
    }

    public async Task<MenuStat> GetItemTypeStats()
    {
        var types = await _context.ItemTypes
            .Where(t => !t.IsDeleted)
            .ToListAsync();
        return new MenuStat
        {
            Available = (uint)types.Count
        };
    }

    public async Task<MenuStat> GetItemTagStats()
    {
        var tags = await _context.ItemTags
            .Where(t => !t.IsDeleted)
            .ToListAsync();
        return new MenuStat
        {
            Available = (uint)tags.Count
        };
    }

    public async Task<UserStat> GetUserStats()
    {
        var users = await _authContext.Users
            .ToListAsync();
        var userStat = new UserStat
        {
            TotalUsers = (uint)users.Count
        };

        foreach (var user in users)
        {
            var roleIds = await _authContext.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .ToListAsync();
            var roles = await _authContext.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync();

            if (roles.Contains(SystemRoles.Admin))
            {
                userStat.Admins++;
                userStat.CanCreate++;
                userStat.CanEdit++;
                userStat.CanDelete++;
                continue;
            }
            if (roles.Contains(SystemRoles.CreatePermission)) userStat.CanCreate++;
            if (roles.Contains(SystemRoles.EditPermission)) userStat.CanEdit++;
            if (roles.Contains(SystemRoles.DeletePermission)) userStat.CanDelete++;
        }
        
        return userStat;
    }
}