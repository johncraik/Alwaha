using AlwahaManagement.Controllers;
using Flurl.Http;

namespace AlwahaManagement.Helpers;

public class GiteaHelper(string url, string apiKey)
{
    private readonly FlurlClient _baseUrl = new FlurlClient(url)
        .WithHeader("Authorization","Bearer "+apiKey)
        .WithHeader("X-GitHub-Api-Version", "2022-11-28")
        .WithHeader("User-Agent", "AlwahaManagement");
    
    public async Task<int> RecordIssue(string owner,string repo,string title,string desc)
    {
        var response = await _baseUrl.Request("repos", owner, repo, "issues").PostJsonAsync(new {title=title,body=desc}).ReceiveJson<NewIssueResponse>();
        return response.Number;
    }
    public class NewCommentResponse
    {
        public long Id { get; set; }
        public string? Url { get; set; }
        public string? Html_Url { get; set; }
    }
    public class NewIssueResponse
    {
        public long Id { get; set; }
        public int Number { get; set; }
        public string? Title { get; set; }
        public string? State { get; set; }
        public string? Html_Url { get; set; }
    }
}