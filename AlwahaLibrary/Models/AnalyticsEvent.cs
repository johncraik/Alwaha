using System.ComponentModel.DataAnnotations;

namespace AlwahaLibrary.Models;

public class AnalyticsEvent
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    [Required]
    [MaxLength(20)]
    public string EventType { get; set; } = string.Empty; // PageView, Click, Submit, etc.

    [Required]
    [MaxLength(2048)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string? Referrer { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(100)]
    public string? SessionId { get; set; }

    public int? ScreenWidth { get; set; }
    public int? ScreenHeight { get; set; }

    [MaxLength(10)]
    public string? Language { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    // Page title
    [MaxLength(500)]
    public string? PageTitle { get; set; }

    // Time spent on page (seconds)
    public int? Duration { get; set; }

    // Additional metadata as JSON
    [MaxLength(2000)]
    public string? Metadata { get; set; }
}
