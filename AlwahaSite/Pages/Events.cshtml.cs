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

    public EventsModel(IEmailSender emailSender,
        EmailBuilderService builderService)
    {
        _emailSender = emailSender;
        _builderService = builderService;
    }

    [BindProperty]
    public EventForm EventForm { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Sorry, there was an error submitting your event inquiry. Please check the form and try again.");
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