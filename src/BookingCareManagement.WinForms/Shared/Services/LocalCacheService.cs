using System;
using System.IO;
using System.Text.Json;

namespace BookingCareManagement.WinForms.Shared.Services;

public sealed class LocalCacheService
{
    private readonly string _cacheDir;
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public LocalCacheService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _cacheDir = Path.Combine(appData, "BookingCareManagement", "WinForms", "cache");
        Directory.CreateDirectory(_cacheDir);
    }

    public void Save<T>(string key, T data)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        var path = BuildPath(key);
        var json = JsonSerializer.Serialize(data, _options);
        File.WriteAllText(path, json);
    }

    public T? Load<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return default;
        }

        var path = BuildPath(key);
        if (!File.Exists(path))
        {
            return default;
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, _options);
        }
        catch
        {
            return default;
        }
    }

    public void Clear(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        var path = BuildPath(key);
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // ignore
        }
    }

    private string BuildPath(string key)
    {
        var safe = string.Concat(key.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_cacheDir, $"{safe}.json");
    }
}
