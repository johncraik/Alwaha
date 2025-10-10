using AlwahaLibrary.Data;
using AlwahaLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlwahaManagement.Services;

public class CloudflareAnalyticsService
{
    private readonly HttpClient _httpClient;
    private readonly AlwahaDbContext _context;
    private readonly IConfiguration _configuration;

    public CloudflareAnalyticsService(HttpClient httpClient, AlwahaDbContext context, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _context = context;
        _configuration = configuration;
    }

    public async Task<bool> SyncCloudflareDataAsync(DateTime startDate, DateTime endDate)
    {
        var apiToken = _configuration["Cloudflare:ApiToken"];
        var zoneId = _configuration["Cloudflare:ZoneId"];

        if (string.IsNullOrEmpty(apiToken) || string.IsNullOrEmpty(zoneId))
        {
            return false;
        }

        var query = $@"
        {{
          viewer {{
            zones(filter: {{zoneTag: $zoneTag}}) {{
              httpRequests1dGroups(
                limit: 1000,
                filter: {{
                  date_geq: ""{startDate:yyyy-MM-dd}"",
                  date_leq: ""{endDate:yyyy-MM-dd}"",
                  clientRequestHTTPHost: ""www.alwahalondon.co.uk""
                }}
              ) {{
                sum {{
                  requests
                  pageViews
                  bytes
                  cachedBytes
                  threats
                }}
                uniq {{
                  uniques
                }}
                avg {{
                  sampleInterval
                }}
                dimensions {{
                  date
                }}
              }}
            }}
          }}
        }}";

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.cloudflare.com/client/v4/graphql");
            request.Headers.Add("Authorization", $"Bearer {apiToken}");
            request.Content = new StringContent(
                JsonSerializer.Serialize(new {
                    query = query,
                    variables = new { zoneTag = zoneId }
                }),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<CloudflareGraphQLResponse>(jsonResponse);

            if (data?.Data?.Viewer?.Zones == null || !data.Data.Viewer.Zones.Any())
            {
                return false;
            }

            var groups = data.Data.Viewer.Zones[0].HttpRequests1dGroups;

            foreach (var group in groups)
            {
                var date = DateTime.Parse(group.Dimensions.Date);
                var existing = await _context.CloudflareAnalytics
                    .FirstOrDefaultAsync(c => c.Date.Date == date.Date);

                var analytics = existing ?? new CloudflareAnalytics { Date = date };

                analytics.Requests = group.Sum.Requests;
                analytics.PageViews = group.Sum.PageViews;
                analytics.UniqueVisitors = group.Uniq.Uniques;
                analytics.BandwidthTotal = group.Sum.Bytes;
                analytics.BandwidthCached = group.Sum.CachedBytes;
                analytics.BandwidthUncached = group.Sum.Bytes - group.Sum.CachedBytes;
                analytics.CacheHitRate = group.Sum.Bytes > 0
                    ? (double)group.Sum.CachedBytes / group.Sum.Bytes * 100
                    : 0;
                analytics.ThreatsBlocked = group.Sum.Threats;
                analytics.AvgOriginResponseTime = group.Avg.SampleInterval;
                analytics.LastSynced = DateTime.UtcNow;

                if (existing == null)
                {
                    await _context.CloudflareAnalytics.AddAsync(analytics);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // GraphQL Response classes
    private class CloudflareGraphQLResponse
    {
        [JsonPropertyName("data")]
        public CloudflareData? Data { get; set; }

        [JsonPropertyName("errors")]
        public object? Errors { get; set; }
    }

    private class CloudflareData
    {
        [JsonPropertyName("viewer")]
        public CloudflareViewer? Viewer { get; set; }
    }

    private class CloudflareViewer
    {
        [JsonPropertyName("zones")]
        public List<CloudflareZone>? Zones { get; set; }
    }

    private class CloudflareZone
    {
        [JsonPropertyName("httpRequests1dGroups")]
        public List<HttpRequestGroup> HttpRequests1dGroups { get; set; } = new();
    }

    private class HttpRequestGroup
    {
        [JsonPropertyName("sum")]
        public RequestSum Sum { get; set; } = new();

        [JsonPropertyName("uniq")]
        public RequestUniq Uniq { get; set; } = new();

        [JsonPropertyName("avg")]
        public RequestAvg Avg { get; set; } = new();

        [JsonPropertyName("dimensions")]
        public RequestDimensions Dimensions { get; set; } = new();
    }

    private class RequestSum
    {
        [JsonPropertyName("requests")]
        public long Requests { get; set; }

        [JsonPropertyName("pageViews")]
        public long PageViews { get; set; }

        [JsonPropertyName("bytes")]
        public long Bytes { get; set; }

        [JsonPropertyName("cachedBytes")]
        public long CachedBytes { get; set; }

        [JsonPropertyName("threats")]
        public long Threats { get; set; }
    }

    private class RequestUniq
    {
        [JsonPropertyName("uniques")]
        public long Uniques { get; set; }
    }

    private class RequestAvg
    {
        [JsonPropertyName("sampleInterval")]
        public double SampleInterval { get; set; }
    }

    private class RequestDimensions
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;
    }
}
