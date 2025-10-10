using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaSite.Pages;

[SitemapIgnore]
public class PrivacyModel : PageModel
{
    private readonly ILogger<PrivacyModel> _logger;

    public PrivacyModel(ILogger<PrivacyModel> logger)
    {
        _logger = logger;
    }
    
    public string? Privacy { get; set; }

    public async Task OnGet()
    {
        Privacy = await System.IO.File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "privacy.txt"));
        Privacy = Privacy.Replace("[your@email.com]", "privacy@alwahalondon.co.uk");
    }
}