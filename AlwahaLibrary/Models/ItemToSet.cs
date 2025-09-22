using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AlwahaLibrary.Models;

[PrimaryKey(nameof(ItemId), nameof(SetId))]
public class ItemToSet : AuditModel
{
    public string ItemId { get; set; } = Guid.NewGuid().ToString();
    [ForeignKey(nameof(ItemId))]
    public MenuItem MenuItem { get; set; }
    
    public string SetId { get; set; } = Guid.NewGuid().ToString();
    [ForeignKey(nameof(SetId))]
    public MenuItem MenuSet { get; set; }
}