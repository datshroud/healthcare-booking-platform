using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Shared.State;

namespace BookingCareManagement.WinForms.Shared.Services;

public sealed class OfflineQueueService
{
    private const string QueueKey = "offline-queue";
    private readonly LocalCacheService _cache;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public OfflineQueueService(LocalCacheService cache)
    {
        _cache = cache;
    }

    public void Enqueue(OfflineRequestEntry entry)
    {
        lock (_cache)
        {
            var list = _cache.Load<List<OfflineRequestEntry>>(QueueKey) ?? new List<OfflineRequestEntry>();
            list.Add(entry);
            _cache.Save(QueueKey, list);
        }
    }

    public async Task TryReplayAsync(IHttpClientFactory httpFactory, CancellationToken cancellationToken = default)
    {
        if (!await _gate.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            var list = _cache.Load<List<OfflineRequestEntry>>(QueueKey) ?? new List<OfflineRequestEntry>();
            if (list.Count == 0)
            {
                return;
            }

            var client = httpFactory.CreateClient("BookingCareApi");
            var remaining = new List<OfflineRequestEntry>();

            foreach (var entry in list)
            {
                try
                {
                    var req = new HttpRequestMessage(new HttpMethod(entry.Method), entry.Url);
                    if (!string.IsNullOrWhiteSpace(entry.Content))
                    {
                        var content = new StringContent(entry.Content, Encoding.UTF8);
                        if (!string.IsNullOrWhiteSpace(entry.ContentType))
                        {
                            content.Headers.ContentType = MediaTypeHeaderValue.Parse(entry.ContentType);
                        }
                        req.Content = content;
                    }

                    using var resp = await client.SendAsync(req, cancellationToken);
                    if (!resp.IsSuccessStatusCode)
                    {
                        remaining.Add(entry);
                    }
                }
                catch
                {
                    remaining.Add(entry);
                    break;
                }
            }

            _cache.Save(QueueKey, remaining);
        }
        finally
        {
            _gate.Release();
        }
    }

    public sealed record OfflineRequestEntry(
        string Method,
        string Url,
        string? Content,
        string? ContentType,
        DateTime CreatedAtUtc
    );
}
