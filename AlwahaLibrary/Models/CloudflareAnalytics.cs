using System.ComponentModel.DataAnnotations;

namespace AlwahaLibrary.Models;

public class CloudflareAnalytics
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Date { get; set; }

    // Request metrics
    public long Requests { get; set; }
    public long PageViews { get; set; }
    public long UniqueVisitors { get; set; }

    // Bandwidth metrics (bytes)
    public long BandwidthTotal { get; set; }
    public long BandwidthCached { get; set; }
    public long BandwidthUncached { get; set; }

    // Cache metrics
    public double CacheHitRate { get; set; }

    // Security metrics
    public long ThreatsBlocked { get; set; }

    // Performance metrics
    public double AvgOriginResponseTime { get; set; }

    // Status code breakdown
    public long Status2xx { get; set; }
    public long Status3xx { get; set; }
    public long Status4xx { get; set; }
    public long Status5xx { get; set; }

    // Metadata
    public DateTime LastSynced { get; set; }
}
