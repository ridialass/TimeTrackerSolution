using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using TimeTracker.Mobile.Services;

namespace TimeTracker.Mobile.Services
{
    /// <summary>
    /// Injecte automatiquement le JWT dans l’en-tête Authorization de chaque requête HTTP.
    /// </summary>
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IMobileAuthService _authService;

        public AuthHeaderHandler(IMobileAuthService authService)
        {
            _authService = authService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Tente de récupérer le token stocké
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
