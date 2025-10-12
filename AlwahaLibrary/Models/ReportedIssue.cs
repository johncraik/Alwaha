namespace AlwahaLibrary.Models;

public enum IssueType
{
    Suggestion,Bug
}
public class ReportedIssue
{
    public int Id { get; set; }
    public IssueType Type { get; set; }
    public string Description { get; set; }
    public byte[]? Image { get; set; }
    public bool ReportSent { get; set; }
    public int? ExternalId { get; set; }
    public int? FixedInBuild { get; set; }
    public bool Closed { get; set; } 
    public DateTime Created { get; set; }
    public string UserId { get; set; }
    public string UserDisplay { get; set; }
}