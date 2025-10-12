namespace AlwahaLibrary.Models;

public class AuditModel
{
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    
    public DateTime? DeletedDate { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? RestoredDate { get; set; }
    public string? RestoredBy { get; set; }
    
    public void FillCreated(string userId)
    {
        CreatedDate = DateTime.UtcNow;
        CreatedBy = userId;
    }

    public void FillUpdated(string userId)
    {
        UpdatedDate = DateTime.UtcNow;
        UpdatedBy = userId;
    }

    public void FillDeleted(string userId)
    {
        DeletedDate = DateTime.UtcNow;
        DeletedBy = userId;
        IsDeleted = true;

        RestoredDate = null;
        RestoredBy = null;
    }

    public void FillRestored(string userId)
    {
        RestoredDate = DateTime.UtcNow;
        RestoredBy = userId;

        IsDeleted = false;
        DeletedDate = null;
        DeletedBy = null;
    }
}