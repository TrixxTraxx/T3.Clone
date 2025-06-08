using System.Text.Json;
using Blazored.LocalStorage;

namespace T3.Clone.Client.Services;

public class SettingsService
{
    private readonly ILocalStorageService _localStorage;
    private const string SettingsKey = "user_settings";

    // Default settings
    public static class Defaults
    {
        // Theme Settings
        public const string PrimaryColor = "#6366f1"; // Primary color
        public const string BackgroundColor = "#070d22"; // Background
        public const string SurfaceColor = "#111936"; // Surface color (slightly lighter than background)
        public const string TextColor = "#FFFFFF"; // Default white text
        public const string SecondaryTextColor = "#B0B0B0"; // Light gray text
    }

    public SettingsService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<T> GetSettingAsync<T>(string key, T defaultValue)
    {
        var settings = await GetAllSettingsAsync();
        
        if (settings.TryGetValue(key, out var value) && value != null)
        {
            try
            {
                // Convert the JsonElement to the requested type
                if (value is JsonElement element)
                {
                    return ConvertJsonElement<T>(element);
                }
                return (T)value;
            }
            catch
            {
                return defaultValue;
            }
        }
        
        return defaultValue;
    }

    public async Task SetSettingAsync<T>(string key, T value)
    {
        var settings = await GetAllSettingsAsync();
        settings[key] = value;
        await _localStorage.SetItemAsync(SettingsKey, settings);
    }

    public async Task<Dictionary<string, object>> GetAllSettingsAsync()
    {
        if (await _localStorage.ContainKeyAsync(SettingsKey))
        {
            return await _localStorage.GetItemAsync<Dictionary<string, object>>(SettingsKey);
        }
        
        return new Dictionary<string, object>();
    }

    private T ConvertJsonElement<T>(JsonElement element)
    {
        if (typeof(T) == typeof(int) && element.ValueKind == JsonValueKind.Number)
        {
            return (T)(object)element.GetInt32();
        }
        else if (typeof(T) == typeof(double) && element.ValueKind == JsonValueKind.Number)
        {
            return (T)(object)element.GetDouble();
        }
        else if (typeof(T) == typeof(bool) && element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False)
        {
            return (T)(object)element.GetBoolean();
        }
        else if (typeof(T) == typeof(string))
        {
            return (T)(object)element.GetString();
        }
        
        // Fallback to deserializing the JSON element
        return JsonSerializer.Deserialize<T>(element.GetRawText());
    }
} 