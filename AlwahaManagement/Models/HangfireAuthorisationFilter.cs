using Hangfire.Dashboard;

namespace AlwahaManagement.Models;

public class HangfireAuthorisationFilter : IDashboardAuthorizationFilter
{
    private readonly string _role;
    
    public HangfireAuthorisationFilter(string role)
    {
        _role = role;
    }
    
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        return http.User.Identity?.IsAuthenticated == true 
               && http.User.IsInRole(_role);
    }
}