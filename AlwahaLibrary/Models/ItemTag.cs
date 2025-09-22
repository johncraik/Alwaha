using System.ComponentModel.DataAnnotations;

namespace AlwahaLibrary.Models;

/// <summary>
/// Allergies and other tags for menu items
/// </summary>
public class ItemTag
{
    [Key]
    public string TagId { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string Name { get; set; }
    public string? Colour { get; set; }
    public string? Icon { get; set; }
}