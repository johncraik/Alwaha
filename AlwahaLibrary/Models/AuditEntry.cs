using System.ComponentModel.DataAnnotations;

namespace AlwahaLibrary.Models;

public class AuditEntry
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; }
    public string EntityId { get; set; }
    public string TableName { get; set; }
    public DateTime Date { get; set; }
    public AuditAction AuditAction { get; set; }
}

public enum AuditAction
{
    CREATE,
    EDIT,
    DELETE,
    RESTORE
}