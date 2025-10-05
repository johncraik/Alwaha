

using System.Text.Json;

namespace AlwahaManagement.Services;

public class SettingsService
{
    private const string FilePath = "settings.json";

    public async Task<Dictionary<string, string>> GetAllSettingsAsync()
    {
        var json = await File.ReadAllTextAsync(FilePath);
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
        var json = await File.ReadAllTextAsync(FilePath);
        var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        settings[key] = value?.ToString() ?? "";
        await File.WriteAllTextAsync(FilePath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
    }

}