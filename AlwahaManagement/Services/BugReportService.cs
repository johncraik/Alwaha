using AlwahaLibrary.Data;
using AlwahaLibrary.Models;
using AlwahaLibrary.Services;
using AlwahaManagement.Controllers;
using AlwahaManagement.Helpers;
using Microsoft.Extensions.Configuration;

namespace AlwahaManagement.Services;

public class BugReportService(AlwahaDbContext _context, 
    GiteaHelper _giteaHelper,
    IConfiguration _configuration)
{
    public async Task<ReportedIssue> RecordIssue(string description, IssueType issueType,
        UserInfo creator)
    {
        var ri = new ReportedIssue
        {
            Description = description,
            Type = issueType,
            Created = DateTime.Now,
            ReportSent = true,
            UserId = creator.UserId ?? "System",
            UserDisplay = creator.UserName ?? "System",
        };

        var issueNumber = await _giteaHelper.RecordIssue(_configuration["Github:Owner"], _configuration["Github:Repo"], "New " + issueType, description);
        ri.ReportSent = true;
        ri.ExternalId = issueNumber;

        await _context.ReportedIssues.AddAsync(ri);
        await _context.SaveChangesAsync();
        return ri;
    }
}