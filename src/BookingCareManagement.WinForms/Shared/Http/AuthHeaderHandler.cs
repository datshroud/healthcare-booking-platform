using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using BookingCareManagement.WinForms.Shared.State;
using BookingCareManagement.WinForms.Shared.Services;


namespace BookingCareManagement.WinForms.Shared.Http
{
    /// <summary>
    /// Injects the current bearer token into outgoing API requests so every area can share the same handler.
    /// </summary>
    public sealed class AuthHeaderHandler : DelegatingHandler
    {
        private readonly SessionState _sessionState;
        private readonly AuthService _authService;
        private readonly IAuthStorage _storage;
        private readonly DialogService _dialogs;
        private static int _notified;
        private static readonly HttpRequestOptionsKey<bool> RetryKey = new("AuthRetry");

        public AuthHeaderHandler(SessionState sessionState, AuthService authService, IAuthStorage storage, DialogService dialogs)
        {
            _sessionState = sessionState;
            _authService = authService;
            _storage = storage;
            _dialogs = dialogs;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_sessionState.AccessToken) && !request.Headers.Contains("Authorization"))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sessionState.AccessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Unauthorized && response.StatusCode != HttpStatusCode.Forbidden)
            {
                return response;
            }

            if (request.Options.TryGetValue(RetryKey, out var retried) && retried)
            {
                return response;
            }

            var refreshed = await _authService.RefreshAccessTokenAsync();
            if (!refreshed)
            {
                if (Interlocked.Exchange(ref _notified, 1) == 0)
                {
                    _dialogs.ShowError("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
                }

                _storage.Clear();
                _sessionState.Clear();
                return response;
            }

            response.Dispose();
            var cloned = await CloneRequestAsync(request);
            cloned.Options.Set(RetryKey, true);
            if (!string.IsNullOrWhiteSpace(_sessionState.AccessToken))
            {
                cloned.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sessionState.AccessToken);
            }
            return await base.SendAsync(cloned, cancellationToken);
        }

        private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (request.Content != null)
            {
                var contentBytes = await request.Content.ReadAsByteArrayAsync();
                var content = new ByteArrayContent(contentBytes);
                foreach (var header in request.Content.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
                clone.Content = content;
            }

            return clone;
        }
    }
}
