using Microsoft.AspNetCore.Mvc;
using Flurl.Http;

namespace AlwahaSite.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IConfiguration configuration, ILogger<AnalyticsController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("track")]
    public async Task<IActionResult> Track([FromBody] AnalyticsEventDto eventDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var apiUrl = _configuration["Analytics:ApiUrl"];
            var apiKey = _configuration["Analytics:ApiKey"];

            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Analytics API URL or API Key not configured");
                return Ok(new { success = false, message = "Analytics not configured" });
            }

            // Forward the request to the management API using Flurl
            // Include original headers so Management API can capture User-Agent, IP, etc.
            var flurlClient = new FlurlClient();
            var response = await flurlClient.Request(apiUrl)
                .WithHeader("X-API-Key", apiKey)
                .WithHeader("User-Agent", Request.Headers.UserAgent.ToString())
                .WithHeader("X-Forwarded-For", Request.Headers["X-Forwarded-For"].ToString())
                .WithHeader("X-Real-IP", Request.Headers["X-Real-IP"].ToString())
                .PostJsonAsync(eventDto)
                .ReceiveJson<ApiResponse>();

            return Ok(new { success = response?.Success ?? false });
        }
        catch (FlurlHttpException ex)
        {
            _logger.LogError(ex, "Failed to send analytics event to management API. Status: {StatusCode}", ex.StatusCode);
            return Ok(new { success = false }); // Return OK to not break client-side
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending analytics event");
            return Ok(new { success = false });
        }
    }
}

public class ApiResponse
{
    public bool Success { get; set; }
}

public class AnalyticsEventDto
{
    public string EventType { get; set; } = "PageView";
    public string Url { get; set; } = string.Empty;
    public string? Referrer { get; set; }
    public string? SessionId { get; set; }
    public int? ScreenWidth { get; set; }
    public int? ScreenHeight { get; set; }
    public string? Language { get; set; }
    public string? PageTitle { get; set; }
    public int? Duration { get; set; }
    public string? Metadata { get; set; }
}