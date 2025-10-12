using AlwahaLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaSite.Pages;

[SitemapIgnore]
public class PrivacyModel : PageModel
{
    private readonly ILogger<PrivacyModel> _logger;
    private readonly SettingsService _settingsService;

    public PrivacyModel(ILogger<PrivacyModel> logger, SettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;
    }
    
    public string? Privacy { get; set; }

    public async Task OnGet()
    {
        var privacyEmail = "privacy@alwahalondon.co.uk";
        try
        {
            privacyEmail = await _settingsService.GetSettingAsync<string>("PrivacyMailbox");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unable to get privacy mailbox from settings. Using default: {privacyEmail}", ex);
        }
        
        Privacy = await System.IO.File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "privacy.txt"));
        Privacy = Privacy.Replace("[your@email.com]", privacyEmail);
    }
}