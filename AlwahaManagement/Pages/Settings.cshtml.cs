using AlwahaLibrary.Authentication;
using AlwahaLibrary.Services;
using AlwahaManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaManagement.Pages;

[Authorize(Roles = SystemRoles.Admin)]
public class SettingsModel : PageModel
{
    private readonly SettingsService _settingsService;
    private readonly IConfiguration _config;

    public SettingsModel(SettingsService settingsService,
        IConfiguration config)
    {
        _settingsService = settingsService;
        _config = config;
    }

    [BindProperty]
    public int AuditLifetimeMonths { get; set; }

    [BindProperty]
    public bool AutoConfirmEmail { get; set; }

    [BindProperty]
    public int MinPasswordLength { get; set; }

    [BindProperty]
    public int GeneratedPasswordLength { get; set; }

    [BindProperty]
    public string InfoMailbox { get; set; } = string.Empty;

    [BindProperty]
    public string EventsMailbox { get; set; } = string.Empty;

    [BindProperty]
    public string PrivacyMailbox { get; set; } = string.Empty;

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        var settings = await _settingsService.GetAllSettingsAsync();

        AuditLifetimeMonths = int.Parse(settings.GetValueOrDefault("AuditLifetimeMonths", "3"));
        AutoConfirmEmail = bool.Parse(settings.GetValueOrDefault("AutoConfirmEmail", "false"));
        MinPasswordLength = int.Parse(settings.GetValueOrDefault("MinPasswordLength", "8"));
        GeneratedPasswordLength = int.Parse(settings.GetValueOrDefault("GeneratedPasswordLength", "16"));
        InfoMailbox = settings.GetValueOrDefault("InfoMailbox", "info@alwahalondon.co.uk");
        EventsMailbox = settings.GetValueOrDefault("EventsMailbox", "events@alwahalondon.co.uk");
        PrivacyMailbox = settings.GetValueOrDefault("PrivacyMailbox", "privacy@alwahalondon.co.uk");
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

        // Validate email addresses are not empty
        if (string.IsNullOrWhiteSpace(InfoMailbox))
        {
            ModelState.AddModelError(nameof(InfoMailbox), "Info mailbox cannot be empty.");
            return Page();
        }

        if (string.IsNullOrWhiteSpace(EventsMailbox))
        {
            ModelState.AddModelError(nameof(EventsMailbox), "Events mailbox cannot be empty.");
            return Page();
        }

        if (string.IsNullOrWhiteSpace(PrivacyMailbox))
        {
            ModelState.AddModelError(nameof(PrivacyMailbox), "Privacy mailbox cannot be empty.");
            return Page();
        }

        // Auto-append domain if not provided
        if (!InfoMailbox.Contains('@')) InfoMailbox = $"{InfoMailbox}@{_config["Domain"]}";
        if (!EventsMailbox.Contains('@')) EventsMailbox = $"{EventsMailbox}@{_config["Domain"]}";
        if (!PrivacyMailbox.Contains('@')) PrivacyMailbox = $"{PrivacyMailbox}@{_config["Domain"]}";

        // Update settings
        await _settingsService.UpdateSettingAsync("AuditLifetimeMonths", AuditLifetimeMonths);
        await _settingsService.UpdateSettingAsync("AutoConfirmEmail", AutoConfirmEmail);
        await _settingsService.UpdateSettingAsync("MinPasswordLength", MinPasswordLength);
        await _settingsService.UpdateSettingAsync("GeneratedPasswordLength", GeneratedPasswordLength);
        await _settingsService.UpdateSettingAsync("InfoMailbox", InfoMailbox);
        await _settingsService.UpdateSettingAsync("EventsMailbox", EventsMailbox);
        await _settingsService.UpdateSettingAsync("PrivacyMailbox", PrivacyMailbox);

        SuccessMessage = "Settings updated successfully!";
        return RedirectToPage();
    }
}