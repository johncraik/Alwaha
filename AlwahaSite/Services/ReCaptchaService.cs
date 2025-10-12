using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlwahaSite.Services;

public class ReCaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReCaptchaService> _logger;

    public ReCaptchaService(HttpClient httpClient, IConfiguration configuration, ILogger<ReCaptchaService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> VerifyTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("reCAPTCHA token is null or empty");
            return false;
        }

        var secretKey = _configuration["ReCaptcha:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            _logger.LogError("reCAPTCHA secret key not configured");
            return false;
        }

        try
        {
            var response = await _httpClient.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = secretKey,
                    ["response"] = token
                })
            );

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ReCaptchaResponse>(jsonResponse);

            if (result == null)
            {
                _logger.LogWarning("Failed to deserialize reCAPTCHA response");
                return false;
            }

            if (!result.Success)
            {
                _logger.LogWarning("reCAPTCHA verification failed. Errors: {Errors}",
                    string.Join(", ", result.ErrorCodes ?? Array.Empty<string>()));
                return false;
            }

            // reCAPTCHA v3 returns a score from 0.0 to 1.0
            // 1.0 is very likely a good interaction, 0.0 is very likely a bot
            // Recommended threshold is 0.5
            if (result.Score < 0.5)
            {
                _logger.LogWarning("reCAPTCHA score too low: {Score}", result.Score);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying reCAPTCHA token");
            return false;
        }
    }

    private class ReCaptchaResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [JsonPropertyName("challenge_ts")]
        public DateTime ChallengeTimestamp { get; set; }

        [JsonPropertyName("hostname")]
        public string? Hostname { get; set; }

        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}