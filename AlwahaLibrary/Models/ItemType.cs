using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlwahaLibrary.Models;

public class ItemType : AuditModel
{
    [Key]
    public string ItemTypeId { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string Name { get; set; }
    public string? Colour { get; set; }
    
    [Required]
    public int Order { get; set; }
    
    public IEnumerable<MenuItem> MenuItems { get; set; }
    public ICollection<BundleItem> BundleItems { get; set; }
}