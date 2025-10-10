using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AlwahaSite.Pages;

public class ContactModel : PageModel
{
    private readonly IEmailSender _emailSender;

    public ContactModel(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }
    
    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Subject is required")]
    [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
    public string Subject { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Message is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters")]
    public string Message { get; set; } = string.Empty;

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // TODO: Add your email sending logic here
        // For now, just simulate success
        try
        {
            await _emailSender.SendEmailAsync("jcraik23@gmail.com", Subject, Message);

            SuccessMessage = "Thank you for contacting us! We'll get back to you soon.";
            return RedirectToPage();
        }
        catch (Exception)
        {
            ErrorMessage = "Sorry, there was an error sending your message. Please try again later.";
            return Page();
        }
    }
}