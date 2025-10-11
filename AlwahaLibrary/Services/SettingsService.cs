

using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AlwahaLibrary.Services;

public class SettingsService
{
    private const string FilePath = "settings.json";
    private readonly string _fullPath;

    public SettingsService(IConfiguration config)
    {
        var basePath = config["BasePath"] ?? throw new Exception("BasePath not found in config");
        _fullPath = Path.Combine(basePath, FilePath);
    }

    public async Task<Dictionary<string, string>> GetAllSettingsAsync()
    {
        var json = await File.ReadAllTextAsync(_fullPath);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
    }

    public async Task<T?> GetSettingAsync<T>(string key)
    {
        var settings = await GetAllSettingsAsync();
        if (settings.TryGetValue(key, out var value))
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return default;
    }

    public async Task UpdateSettingAsync<T>(string key, T value)
    {
        var json = await File.ReadAllTextAsync(_fullPath);
        var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        settings[key] = value?.ToString() ?? "";
        await File.WriteAllTextAsync(_fullPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
    }

}