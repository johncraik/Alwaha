using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace AlwahaLibrary.Middleware;

public static class UserInfoMiddlewareExtensions
{
    public static IApplicationBuilder UseUserInfo(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserInfoMiddleware>();
    }
}

public class UserInfoMiddleware
{
    private readonly RequestDelegate _next;

    public UserInfoMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var userInfo = (UserInfo)context.RequestServices.GetRequiredService(typeof(UserInfo));
        var io = context.RequestServices.GetRequiredService<IOptions<IdentityOptions>>();
        if (!userInfo.IsSetup)
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                userInfo.UserName = "System";
            }
            else
            {
                userInfo.UserName = context.User.Identity.Name;
                userInfo.Email = context.User.FindFirst(io.Value.ClaimsIdentity.EmailClaimType)?.Value;
                userInfo.UserId = context.User.FindFirst(io.Value.ClaimsIdentity.UserIdClaimType)?.Value;

                // Populate roles from claims
                var roleClaims = context.User.FindAll(io.Value.ClaimsIdentity.RoleClaimType);
                userInfo.Roles = roleClaims.Select(c => c.Value).ToList();

                if (string.IsNullOrWhiteSpace(userInfo.UserId))
                {
                    throw new InvalidOperationException("No userid in userinfo");
                }
            }

            userInfo.IsSetup = true;
        }

        await _next(context);
    }
}