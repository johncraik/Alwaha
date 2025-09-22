using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlwahaLibrary.Models;

public class MenuItem : AuditModel
{
    [Key] 
    public string ItemId { get; set; } = Guid.NewGuid().ToString();

    [Required] public string ItemTypeId { get; set; }
    [ForeignKey(nameof(ItemTypeId))] 
    public ItemType ItemType { get; set; }

    [Required] 
    public string Name { get; set; }
    public string? Description { get; set; }
    [Required] 
    public double Price { get; set; }

    public bool IsAvailable { get; set; } = true;


    public bool IsSet { get; set; }

    public string? ImagePath { get; set; }
    public string? Colour { get; set; }

    public ICollection<BundleItem> BundleItems { get; set; }
    public ICollection<MenuItem> SetItems { get; set; }
}