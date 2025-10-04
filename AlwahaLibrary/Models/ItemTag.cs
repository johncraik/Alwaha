using System.ComponentModel.DataAnnotations;

namespace AlwahaLibrary.Models;

/// <summary>
/// Allergies and other tags for menu items
/// </summary>
public class ItemTag : AuditModel
{
    [Key]
    public string TagId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string Name { get; set; }

    [Required]
    public string Colour { get; set; }

    [Required]
    public string Icon { get; set; }
    
    public ICollection<ItemToTag> ItemsToTags { get; set; }
}