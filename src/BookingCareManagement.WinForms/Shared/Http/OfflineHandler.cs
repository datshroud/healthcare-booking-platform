using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Shared.Services;
using BookingCareManagement.WinForms.Shared.State;

namespace BookingCareManagement.WinForms.Shared.Http;

public sealed class OfflineHandler : DelegatingHandler
{
    private readonly OfflineStateService _offlineState;
    private readonly OfflineCacheService _cache;
    private readonly OfflineQueueService _queue;
    private readonly SessionState _session;
    private readonly DialogService _dialogs;
    private readonly IHttpClientFactory _httpFactory;

    public OfflineHandler(
        OfflineStateService offlineState,
        OfflineCacheService cache,
        OfflineQueueService queue,
        SessionState session,
        DialogService dialogs,
        IHttpClientFactory httpFactory)
    {
        _offlineState = offlineState;
        _cache = cache;
        _queue = queue;
        _session = session;
        _dialogs = dialogs;
        _httpFactory = httpFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var isReadOnly = IsReadOnlyRequest(request);
        string? requestBody = null;
        string? contentType = request.Content?.Headers?.ContentType?.ToString();
        var isMultipart = IsMultipart(contentType);

        if (!isMultipart && request.Content != null && (isReadOnly || request.Method != HttpMethod.Get))
        {
            requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            var mediaType = NormalizeMediaType(contentType) ?? "application/json";
            var newContent = new StringContent(requestBody, Encoding.UTF8, mediaType);
            foreach (var header in request.Content.Headers)
            {
                if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            request.Content = newContent;
        }

        var cacheKey = BuildCacheKey(request, requestBody, isReadOnly);
        if (!NetworkInterface.GetIsNetworkAvailable())
        {
            if (_offlineState.MarkOffline("network"))
            {
                _offlineState.TryNotifyOffline(_dialogs);
            }

            return await HandleOfflineAsync(request, cacheKey, requestBody, isReadOnly, isMultipart);
        }

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode && isReadOnly)
            {
                await CacheResponseAsync(cacheKey, response);
            }

            if (_offlineState.MarkOnline())
            {
                await _queue.TryReplayAsync(_httpFactory, cancellationToken);
            }

            return response;
        }
        catch (HttpRequestException)
        {
            if (_offlineState.MarkOffline("network"))
            {
                _offlineState.TryNotifyOffline(_dialogs);
            }
            return await HandleOfflineAsync(request, cacheKey, requestBody, isReadOnly, isMultipart);
        }
    }

    private async Task<HttpResponseMessage> HandleOfflineAsync(HttpRequestMessage request, string cacheKey, string? requestBody, bool isReadOnly, bool isMultipart)
    {
        if (isReadOnly)
        {
            var cached = _cache.Load(cacheKey);
            if (cached != null)
            {
                var mediaType = NormalizeMediaType(cached.ContentType) ?? "application/json";
                var resp = new HttpResponseMessage((HttpStatusCode)cached.StatusCode)
                {
                    Content = new StringContent(cached.Content ?? string.Empty, Encoding.UTF8, mediaType),
                    ReasonPhrase = "Offline cache"
                };
                return resp;
            }

            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("{\"error\":\"offline\"}", Encoding.UTF8, "application/json"),
                ReasonPhrase = "Offline"
            };
        }

        if (IsAuthEndpoint(request))
        {
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("{\"error\":\"offline\"}", Encoding.UTF8, "application/json"),
                ReasonPhrase = "Offline"
            };
        }

        if (isMultipart)
        {
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("{\"error\":\"offline\",\"detail\":\"multipart-not-supported\"}", Encoding.UTF8, "application/json"),
                ReasonPhrase = "Offline"
            };
        }

        var body = requestBody ?? (request.Content is null ? null : await request.Content.ReadAsStringAsync());
        var contentType = request.Content?.Headers?.ContentType?.ToString();
        _queue.Enqueue(new OfflineQueueService.OfflineRequestEntry(
            request.Method.Method,
            request.RequestUri?.ToString() ?? string.Empty,
            body,
            contentType,
            DateTime.UtcNow));

        return new HttpResponseMessage(HttpStatusCode.Accepted)
        {
            Content = new StringContent("{\"queued\":true,\"offline\":true}", Encoding.UTF8, "application/json"),
            ReasonPhrase = "Queued (offline)"
        };
    }

    private async Task CacheResponseAsync(string cacheKey, HttpResponseMessage response)
    {
        var content = response.Content is null ? null : await response.Content.ReadAsStringAsync();
        var contentType = NormalizeMediaType(response.Content?.Headers?.ContentType?.ToString());
        _cache.Save(cacheKey, new OfflineCacheService.OfflineCacheEntry(
            (int)response.StatusCode,
            content,
            contentType,
            DateTime.UtcNow));
    }

    private string BuildCacheKey(HttpRequestMessage request, string? requestBody, bool isReadOnly)
    {
        var userKey = string.IsNullOrWhiteSpace(_session.CurrentUserId) ? _session.Email : _session.CurrentUserId;
        var url = request.RequestUri?.ToString() ?? string.Empty;
        if (request.Method == HttpMethod.Get || !isReadOnly)
        {
            return $"{userKey}:{request.Method}:{url}";
        }

        var bodyHash = string.IsNullOrWhiteSpace(requestBody)
            ? string.Empty
            : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(requestBody)));

        return $"{userKey}:{request.Method}:{url}:{bodyHash}";
    }

    private static bool IsAuthEndpoint(HttpRequestMessage request)
    {
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        return path.Contains("/api/account/auth", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsReadOnlyRequest(HttpRequestMessage request)
    {
        return request.Method == HttpMethod.Get || IsReadOnlyPostEndpoint(request);
    }

    private static bool IsReadOnlyPostEndpoint(HttpRequestMessage request)
    {
        if (request.Method != HttpMethod.Post)
        {
            return false;
        }

        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        return path.Contains("/api/customer/search", StringComparison.OrdinalIgnoreCase)
               || path.Contains("/api/customer-booking/search", StringComparison.OrdinalIgnoreCase)
               || path.Contains("/api/admin/appointments/search", StringComparison.OrdinalIgnoreCase)
               || path.Contains("/api/doctor/appointments/search", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMultipart(string? contentType)
    {
        return !string.IsNullOrWhiteSpace(contentType)
               && contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeMediaType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return null;
        }

        var semicolon = contentType.IndexOf(';');
        return semicolon > 0 ? contentType.Substring(0, semicolon).Trim() : contentType.Trim();
    }
}
