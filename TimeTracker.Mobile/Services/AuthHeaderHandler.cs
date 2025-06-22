using Microsoft.Maui.Storage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace TimeTracker.Mobile.Services
{
    /// <summary>
    /// Injecte automatiquement le JWT dans l’en-tête Authorization de chaque requête HTTP.
    /// </summary>
    public class AuthHeaderHandler : DelegatingHandler
    {

        private readonly ISecureStorageService _secureStorage;

        public AuthHeaderHandler(ISecureStorageService secureStorage)
        {
            _secureStorage = secureStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var token = await _secureStorage.GetAsync("jwt_token");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, ct);
        }
    }
}

