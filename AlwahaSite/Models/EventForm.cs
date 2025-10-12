using System.ComponentModel.DataAnnotations;

namespace AlwahaSite.Models;

public class EventForm : ContactForm
{
    public new string Subject { get; set; } = "Event";
    
    [Required(ErrorMessage = "Party size is required")]
    [Range(1, 20)]
    public uint PartySize { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public EventForm()
    {
    }

    public EventForm(string email, string message, uint partySize, DateTime date) 
        : base(email, "Event", message)
    {
        PartySize = partySize;
        Date = date;
    }
}