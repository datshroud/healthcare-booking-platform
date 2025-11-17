using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using BookingCareManagement.WinForms.Shared.State;

namespace BookingCareManagement.WinForms.Shared.Http
{
    /// <summary>
    /// Injects the current bearer token into outgoing API requests so every area can share the same handler.
    /// </summary>
    public sealed class AuthHeaderHandler : DelegatingHandler
    {
        private readonly SessionState _sessionState;

        public AuthHeaderHandler(SessionState sessionState)
        {
            _sessionState = sessionState;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_sessionState.IsAuthenticated && !request.Headers.Contains("Authorization"))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sessionState.AccessToken);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
