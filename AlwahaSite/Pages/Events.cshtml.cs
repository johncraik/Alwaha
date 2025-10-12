using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AlwahaSite.Models;
using AlwahaSite.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AlwahaSite.Pages;

public class EventsModel : PageModel
{
    private readonly IEmailSender _emailSender;
    private readonly EmailBuilderService _builderService;
    private readonly ReCaptchaService _reCaptchaService;
    private readonly IConfiguration _configuration;

    public EventsModel(IEmailSender emailSender,
        EmailBuilderService builderService,
        ReCaptchaService reCaptchaService,
        IConfiguration configuration)
    {
        _emailSender = emailSender;
        _builderService = builderService;
        _reCaptchaService = reCaptchaService;
        _configuration = configuration;
    }

    public string ReCaptchaSiteKey => _configuration["ReCaptcha:SiteKey"] ?? "";

    [BindProperty]
    public EventForm EventForm { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string recaptchaToken)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Sorry, there was an error submitting your event inquiry. Please check the form and try again.");
            return Page();
        }

        // Verify reCAPTCHA
        var isValidCaptcha = await _reCaptchaService.VerifyTokenAsync(recaptchaToken);
        if (!isValidCaptcha)
        {
            ModelState.AddModelError(string.Empty, "reCAPTCHA verification failed. Please try again.");
            return Page();
        }

        try
        {
            var response = await _builderService.Build(EventForm, isEvent: true);
            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, "Sorry, there was an error submitting your event inquiry. Please try again later.");
                return Page();
            }

            await _emailSender.SendEmailAsync(response.SendTo!, response.Subject!, response.Body!);

            SuccessMessage = "Thank you for your event inquiry! We'll get back to you soon to discuss the details.";
            return RedirectToPage();
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Sorry, there was an error submitting your event inquiry. Please try again later.");
            return Page();
        }
    }
}