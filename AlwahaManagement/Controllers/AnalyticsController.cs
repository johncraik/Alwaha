using AlwahaLibrary.Data;
using AlwahaLibrary.Helpers;
using AlwahaLibrary.Models;
using AlwahaManagement.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlwahaManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly AlwahaDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly GeoLocationHelper _geoLocation;

    public AnalyticsController(AlwahaDbContext context, IConfiguration configuration, GeoLocationHelper geoLocation)
    {
        _context = context;
        _configuration = configuration;
        _geoLocation = geoLocation;
    }

    [AllowAnonymous]
    [HttpPost("track")]
    public async Task<IActionResult> Track([FromBody] AnalyticsEventDto eventDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var clientIp = GetClientIp();
        var ipHashSalt = _configuration["Analytics:IpHashSalt"] ?? "--no_salt_found--";
        var hashedIp = IpHashHelper.HashIpAddressWithDateRotation(clientIp, ipHashSalt);

        // Get geolocation from IP (async)
        var (country, city) = await _geoLocation.GetLocationFromIpAsync(clientIp);

        var analyticsEvent = new AnalyticsEvent
        {
            Timestamp = DateTime.UtcNow,
            EventType = eventDto.EventType,
            Url = eventDto.Url,
            Referrer = eventDto.Referrer,
            UserAgent = Request.Headers.UserAgent.ToString(),
            IpAddress = hashedIp, // Store hashed IP instead of raw IP
            SessionId = eventDto.SessionId,
            ScreenWidth = eventDto.ScreenWidth,
            ScreenHeight = eventDto.ScreenHeight,
            Language = eventDto.Language,
            PageTitle = eventDto.PageTitle,
            Duration = eventDto.Duration,
            Metadata = eventDto.Metadata,
            Country = country,
            City = city
        };

        await _context.AnalyticsEvents.AddAsync(analyticsEvent);
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    private string? GetClientIp()
    {
        // Check for forwarded IP first (if behind a proxy/load balancer)
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ip = forwardedFor.ToString().Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(ip))
                return ip;
        }

        if (Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            return realIp.ToString();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
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
