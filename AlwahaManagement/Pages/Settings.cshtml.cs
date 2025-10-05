using AlwahaLibrary.Authentication;
using AlwahaManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages;

[Authorize(Roles = SystemRoles.Admin)]
public class SettingsModel : PageModel
{
    private readonly SettingsService _settingsService;

    public SettingsModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [BindProperty]
    public int AuditLifetimeMonths { get; set; }

    [BindProperty]
    public bool AutoConfirmEmail { get; set; }

    [BindProperty]
    public int MinPasswordLength { get; set; }

    [BindProperty]
    public int GeneratedPasswordLength { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        var settings = await _settingsService.GetAllSettingsAsync();

        AuditLifetimeMonths = int.Parse(settings.GetValueOrDefault("AuditLifetimeMonths", "3"));
        AutoConfirmEmail = bool.Parse(settings.GetValueOrDefault("AutoConfirmEmail", "false"));
        MinPasswordLength = int.Parse(settings.GetValueOrDefault("MinPasswordLength", "8"));
        GeneratedPasswordLength = int.Parse(settings.GetValueOrDefault("GeneratedPasswordLength", "16"));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Validate values
        if (AuditLifetimeMonths < 1 || AuditLifetimeMonths > 120)
        {
            ModelState.AddModelError(nameof(AuditLifetimeMonths), "Audit lifetime must be between 1 and 120 months.");
            return Page();
        }

        if (MinPasswordLength < 6 || MinPasswordLength > 32)
        {
            ModelState.AddModelError(nameof(MinPasswordLength), "Minimum password length must be between 6 and 32 characters.");
            return Page();
        }

        if (GeneratedPasswordLength < MinPasswordLength || GeneratedPasswordLength > 64)
        {
            ModelState.AddModelError(nameof(GeneratedPasswordLength), $"Generated password length must be between {MinPasswordLength} and 64 characters.");
            return Page();
        }

        // Update settings
        await _settingsService.UpdateSettingAsync("AuditLifetimeMonths", AuditLifetimeMonths);
        await _settingsService.UpdateSettingAsync("AutoConfirmEmail", AutoConfirmEmail);
        await _settingsService.UpdateSettingAsync("MinPasswordLength", MinPasswordLength);
        await _settingsService.UpdateSettingAsync("GeneratedPasswordLength", GeneratedPasswordLength);

        SuccessMessage = "Settings updated successfully!";
        return RedirectToPage();
    }
}