using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlwahaLibrary.Models;

public class BundleItem : AuditModel
{
    [Key]
    public string BundleId { get; set; } = Guid.NewGuid().ToString();
    
    public string ItemId { get; set; }
    [ForeignKey(nameof(ItemId))]
    public MenuItem MenuItem { get; set; }
    
    public int Quantity { get; set; }
    public double Price { get; set; }
}