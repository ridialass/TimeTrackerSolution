using System.Net.Http;
using System.Net.Http.Headers;
using TimeTracker.Mobile.Services.Interfaces;

namespace TimeTracker.Mobile.Services
{
    /// <summary>
    /// Adds "Authorization: Bearer {token}" to outgoing requests.
    /// Reads "auth_access_token" (primary), then "jwt_token" (compat).
    /// Skips known auth endpoints (login/register) and respects any pre-set header.
    /// </summary>
    public sealed class AuthHeaderHandler : DelegatingHandler
    {
        private static readonly string[] _authBypassPaths =
        [
            "/api/auth/login",
            "/api/auth/register",
            "/auth/login",
            "/auth/register"
        ];

        private readonly ISecureStorageService _secureStorage;

        public AuthHeaderHandler(ISecureStorageService secureStorage)
            => _secureStorage = secureStorage;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Respect an existing Authorization header (e.g., for special cases)
            if (request.Headers.Authorization is not null)
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // Ignore auth endpoints
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (!string.IsNullOrEmpty(path) && _authBypassPaths.Any(p => path.Contains(p, StringComparison.OrdinalIgnoreCase)))
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // Fetch token
            var token = await _secureStorage.GetAsync(AuthService.TokenStorageKeys.AccessToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(token))
                token = await _secureStorage.GetAsync("jwt_token").ConfigureAwait(false); // legacy

            token = token?.Trim();
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}