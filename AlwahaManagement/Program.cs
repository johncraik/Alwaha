using AlwahaLibrary.Authentication;
using AlwahaLibrary.Data;
using AlwahaLibrary.Middleware;
using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AlwahaManagement.Data;
using AlwahaManagement.Hangfire;
using AlwahaManagement.Helpers;
using AlwahaManagement.Models;
using AlwahaManagement.Services;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEmailSender, EmailSender>();

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

var hangfireConnection = builder.Configuration.GetConnectionString("HangfireConnection") ??
                         throw new InvalidOperationException("Connection string 'HangfireConnection' not found.");
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(hangfireConnection));

// Load settings from settings.json
var settingsPath = Path.Combine(builder.Environment.ContentRootPath, "settings.json");
var settingsJson = File.Exists(settingsPath) ? File.ReadAllText(settingsPath) : "{}";
var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(settingsJson) ?? new Dictionary<string, string>();
var minPasswordLength = settings.TryGetValue("MinPasswordLength", out var minPwdLen) ? int.Parse(minPwdLen) : 8;

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Sign-in settings
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = true;

    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = minPasswordLength;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.NoCache()); // Default: no cache
    options.AddPolicy("Analytics", builder => builder.Expire(TimeSpan.FromMinutes(5)));
});

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
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddHttpClient<CloudflareAnalyticsService>();
builder.Services.AddHttpClient<GeoLocationHelper>();

//Hangfire
builder.Services.AddScoped<RecurringJobs>();
builder.Services.AddScoped<AuditCleanupJob>();
builder.Services.AddScoped<CloudflareSyncJob>();
builder.Services.AddScoped<AnalyticsCleanupJob>();

builder.Services.AddScoped<BugReportService>();
builder.Services.AddSingleton(x =>
    new GiteaHelper(builder.Configuration["Github:Url"], builder.Configuration["Github:ApiKey"]));

// CORS for AlwahaSite
builder.Services.AddCors(options =>
{
    options.AddPolicy("AlwahaSite", policy =>
    {
        policy.WithOrigins(builder.Configuration["Analytics:AllowedOrigins"]?.Split(',') ?? new[] { "*" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-API-Key");
    });
});

// Add Syncfusion services
var syncfusionLicenseKey = builder.Configuration.GetSection("SYNCFUSION-KEY").Value;
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);;

builder.Services.AddHangfireServer();

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

app.UseOutputCache();

app.UseCors("AlwahaSite");

app.UseAuthentication();
app.UseAuthorization();
app.UseUserInfo();
app.UseRequire2FA();

app.UseHangfireDashboard(options: new DashboardOptions{Authorization = [new HangfireAuthorisationFilter(SystemRoles.Admin)]});

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
    
    var jobs = sp.GetRequiredService<RecurringJobs>();
    jobs.RegisterJobs();
    
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
    await ConfirmRoleSetup(SystemRoles.RestorePermission);
    
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