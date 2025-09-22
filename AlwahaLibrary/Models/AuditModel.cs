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
    
    public void FillCreated(string userId)
    {
        CreatedDate = DateTime.Now;
        CreatedBy = userId;
    }

    public void FillUpdated(string userId)
    {
        UpdatedDate = DateTime.Now;
        UpdatedBy = userId;
    }

    public void FillDeleted(string userId)
    {
        DeletedDate = DateTime.Now;
        DeletedBy = userId;
        IsDeleted = true;
    }
    
    public void Restore()
    {
        IsDeleted = false;
        DeletedDate = null;
        DeletedBy = null;
    }
}