using AlwahaLibrary.Data;
using AlwahaLibrary.Services;
using AlwahaSite.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AlwahaDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers();

builder.Services.AddRazorPages(options =>
{
    // Map the same page to both endpoints
    options.Conventions.AddPageRoute("/sitemap", "sitemap.xml");
    options.Conventions.AddPageRoute("/robots", "robots.txt");
});

builder.Services.TryAddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<UserInfo>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<ItemTypeService>();
builder.Services.AddScoped<ItemTagService>();
builder.Services.AddScoped<BundleService>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<EmailSanitiseService>();
builder.Services.AddScoped<EmailBuilderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();