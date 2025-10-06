using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AlwahaLibrary.Middleware;

public class Require2FAMiddleware
{
    private readonly RequestDelegate _next;

    public Require2FAMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userManager = context.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
            var user = await userManager.GetUserAsync(context.User);

            if (user != null && !user.TwoFactorEnabled)
            {
                var path = context.Request.Path.Value ?? "";
                Console.WriteLine($"====URL: {path} ====");

                // Allow access to 2FA setup pages and account pages
                if (!path.Contains("enableauthenticator", StringComparison.OrdinalIgnoreCase) &&
                    !path.Contains("twofactorauthentication", StringComparison.OrdinalIgnoreCase) &&
                    !path.Contains("showrecoverycodes", StringComparison.OrdinalIgnoreCase) &&
                    !path.Contains("logout", StringComparison.OrdinalIgnoreCase) &&
                    !path.Contains("accessdenied", StringComparison.OrdinalIgnoreCase) &&
                    !IsStaticFile(path))
                {
                    context.Response.Redirect("/Identity/Account/Manage/TwoFactorAuthentication");
                    return;
                }
            }
        }

        await _next(context);
    }
    
    private bool IsStaticFile(string path)
    {
        // Check if path is for static resources
        if (path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/fonts/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/_framework/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/_content/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        // Check for common static file extensions
        var extension = Path.GetExtension(path);
        if (!string.IsNullOrEmpty(extension))
        {
            var staticExtensions = new[] { ".css", ".js", ".jpg", ".jpeg", ".png", ".gif", ".svg", ".ico", ".woff", ".woff2", ".ttf", ".eot", ".map", ".json" };
            return staticExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }
        
        return false;
    }
}

public static class Require2FAMiddlewareExtensions
{
    public static IApplicationBuilder UseRequire2FA(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<Require2FAMiddleware>();
    }
}