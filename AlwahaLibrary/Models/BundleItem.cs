using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlwahaLibrary.Models;

public class BundleItem : AuditModel
{
    [Key]
    public string BundleId { get; set; } = Guid.NewGuid().ToString();
    
    public string ItemTypeId { get; set; }
    [ForeignKey(nameof(ItemTypeId))]
    public ItemType ItemType { get; set; }
    
    public int Quantity { get; set; }
    public double Price { get; set; }
}