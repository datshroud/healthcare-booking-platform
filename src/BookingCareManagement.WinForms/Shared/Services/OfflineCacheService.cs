using System;
using System.Collections.Generic;
using BookingCareManagement.WinForms.Shared.State;

namespace BookingCareManagement.WinForms.Shared.Services;

public sealed class OfflineCacheService
{
    private readonly LocalCacheService _cache;

    public OfflineCacheService(LocalCacheService cache)
    {
        _cache = cache;
    }

    public void Save(string key, OfflineCacheEntry entry)
    {
        _cache.Save(key, entry);
    }

    public OfflineCacheEntry? Load(string key)
    {
        return _cache.Load<OfflineCacheEntry>(key);
    }

    public sealed record OfflineCacheEntry(
        int StatusCode,
        string? Content,
        string? ContentType,
        DateTime UpdatedAtUtc
    );
}
