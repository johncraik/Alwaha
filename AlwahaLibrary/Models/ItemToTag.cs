using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AlwahaLibrary.Models;

[PrimaryKey(nameof(ItemId), nameof(TagId))]
public class ItemToTag
{
    public string ItemId { get; set; }
    [ForeignKey(nameof(ItemId))]
    public MenuItem MenuItem { get; set; }
    
    public string TagId { get; set; }
    [ForeignKey(nameof(TagId))]
    public ItemTag ItemTag { get; set; }
}