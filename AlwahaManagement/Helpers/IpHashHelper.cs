using System.Security.Cryptography;
using System.Text;

namespace AlwahaManagement.Helpers;

public static class IpHashHelper
{
    /// <summary>
    /// Hashes an IP address with a salt for privacy-preserving analytics
    /// </summary>
    /// <param name="ipAddress">The IP address to hash</param>
    /// <param name="salt">A secret salt value (store in appsettings)</param>
    /// <returns>A hashed representation of the IP address</returns>
    public static string HashIpAddress(string? ipAddress, string salt)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return string.Empty;

        // Combine IP with salt
        var input = $"{ipAddress}:{salt}";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Hash using SHA256
        var hash = SHA256.HashData(bytes);

        // Convert to base64 for storage (shorter than hex)
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Hashes an IP address with date rotation for daily unique visitor counting
    /// This allows counting unique visitors per day without storing actual IPs
    /// </summary>
    /// <param name="ipAddress">The IP address to hash</param>
    /// <param name="salt">A secret salt value</param>
    /// <param name="date">The date for rotation (defaults to today)</param>
    /// <returns>A date-specific hashed IP</returns>
    public static string HashIpAddressWithDateRotation(string? ipAddress, string salt, DateTime? date = null)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return string.Empty;

        var targetDate = date ?? DateTime.UtcNow.Date;

        // Combine IP with salt and date
        var input = $"{ipAddress}:{salt}:{targetDate:yyyy-MM-dd}";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Hash using SHA256
        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }
}
