using AlwahaLibrary.Authentication;
using AlwahaLibrary.Data;
using AlwahaLibrary.Middleware;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AlwahaManagement.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AlwahaDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var authConnectionString = builder.Configuration.GetConnectionString("AuthenticationConnection") ??
                   throw new InvalidOperationException("Connection string 'AuthenticationConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(authConnectionString, ServerVersion.AutoDetect(authConnectionString)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Logout");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
    options.Conventions.AllowAnonymousToPage("/Account/Lockout");
    options.Conventions.AllowAnonymousToPage("/Account/ForgotPassword");
    options.Conventions.AllowAnonymousToPage("/Account/ForgotPasswordConfirmation");
    options.Conventions.AllowAnonymousToPage("/Account/ResetPassword");
    options.Conventions.AllowAnonymousToPage("/Account/ResetPasswordConfirmation");
});


builder.Services.AddScoped<UserInfo>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<ItemTypeService>();
builder.Services.AddScoped<ItemTagService>();
builder.Services.AddScoped<BundleService>();

// Add Syncfusion services
var syncfusionLicenseKey = builder.Configuration.GetSection("SYNCFUSION-KEY").Value;
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);;


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();
app.UseUserInfo();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}");
app.MapRazorPages()
    .WithStaticAssets();

await Defaults();
app.Run();

async Task Defaults()
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;
    var authDb = sp.GetRequiredService<ApplicationDbContext>();
    var alwahaDb = sp.GetRequiredService<AlwahaDbContext>();
    
    var userManager = sp.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

    await authDb.Database.MigrateAsync();
    await alwahaDb.Database.MigrateAsync();

    async Task ConfirmRoleSetup(string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    
    await ConfirmRoleSetup(SystemRoles.Admin);
    await ConfirmRoleSetup(SystemRoles.CreatePermission);
    await ConfirmRoleSetup(SystemRoles.EditPermission);
    await ConfirmRoleSetup(SystemRoles.DeletePermission);
    
    var adminUser = await userManager.FindByNameAsync("systemadmin");
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            Email = "cibellealvesharb@hotmail.com",
            UserName = "systemadmin",
            EmailConfirmed = true,
            TwoFactorEnabled = false
        };
        await userManager.CreateAsync(adminUser);
        await userManager.AddToRoleAsync(adminUser, SystemRoles.Admin);
        var p = "Admin@1234!";
        await userManager.AddPasswordAsync(adminUser, p);
    }
    else if (!await userManager.IsInRoleAsync(adminUser, SystemRoles.Admin))
    {
        await userManager.AddToRoleAsync(adminUser, SystemRoles.Admin);   
    }
}