using System.Text.Json;

namespace AlwahaManagement.Helpers;

public class GeoLocationHelper
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeoLocationHelper> _logger;

    public GeoLocationHelper(HttpClient httpClient, IConfiguration configuration, ILogger<GeoLocationHelper> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Gets geographic location from IP address using ip-api.com (free, no key required)
    /// Rate limit: 45 requests per minute
    /// </summary>
    public async Task<(string? Country, string? City)> GetLocationFromIpAsync(string? ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1" || ipAddress.StartsWith("127.") || ipAddress.StartsWith("192.168."))
        {
            return ("Local", "Local");
        }

        try
        {
            // Using ip-api.com free service (no API key needed)
            // Format: http://ip-api.com/json/{ip}?fields=country,city
            var response = await _httpClient.GetAsync($"http://ip-api.com/json/{ipAddress}?fields=status,country,city");

            if (!response.IsSuccessStatusCode)
                return (null, null);

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<IpApiResponse>(json);

            if (data?.Status == "success")
            {
                return (data.Country, data.City);
            }

            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get geolocation for IP: {IpAddress}", ipAddress);
            return (null, null);
        }
    }

    private class IpApiResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string? Status { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("country")]
        public string? Country { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("city")]
        public string? City { get; set; }
    }
}
