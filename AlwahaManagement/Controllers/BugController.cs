using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using AlwahaManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlwahaManagement.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class BugController(BugReportService _bugReportService,
    UserInfo userInfo) : ControllerBase
{
    public class BugReportPost
    {
        public string Type { get; set;}
        public string Description { get; set; }
    }

    public class BugReportFile
    {
        public string? FileBytes { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public byte[]? FileContent { get; set; }
    }
    
    [HttpPost]
    public async Task<ActionResult<bool>> ReportBug(BugReportPost postData)
    {
        try
        {
            await _bugReportService.RecordIssue(postData.Description,
                postData.Type.Equals("bug",StringComparison.CurrentCultureIgnoreCase) ? IssueType.Bug : IssueType.Suggestion,
                userInfo);
        }
        catch (Exception e)
        {
            return BadRequest();
        }
        return Ok();
    }
    
}