using System.ComponentModel.DataAnnotations;

namespace AlwahaSite.Models;

public class ContactForm
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Subject is required")]
    [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
    public string Subject { get; set; }
    
    [Required(ErrorMessage = "Message is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters")]
    public string Message { get; set; }

    public ContactForm()
    {
    }

    public ContactForm(string email, string subject, string message)
    {
        Email = email;
        Subject = subject;
        Message = message;
    }
}