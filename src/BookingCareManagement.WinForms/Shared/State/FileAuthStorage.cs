using System;
using System.IO;
using System.Text.Json;

namespace BookingCareManagement.WinForms.Shared.State;

public sealed class FileAuthStorage : IAuthStorage
{
    private readonly string _filePath;

    public FileAuthStorage()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dir = Path.Combine(appData, "BookingCareManagement", "WinForms");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "auth.json");
    }

    public void Save(SessionSnapshot snapshot)
    {
        var json = JsonSerializer.Serialize(snapshot);
        File.WriteAllText(_filePath, json);
    }

    public SessionSnapshot? Load()
    {
        if (!File.Exists(_filePath)) return null;
        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<SessionSnapshot>(json);
        }
        catch
        {
            return null;
        }
    }

    public void Clear()
    {
        try { if (File.Exists(_filePath)) File.Delete(_filePath); } catch { }
    }
}
