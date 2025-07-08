using Microsoft.Maui.Storage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System;

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
            try
            {
                var token = await _secureStorage.GetAsync("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                return await base.SendAsync(request, ct);
            }
            catch (OperationCanceledException ex)
            {
                Debug.WriteLine($"[AuthHeaderHandler] Requête annulée : {ex.Message}");
                // Important: rethrow cancellation so upstream can honor it
                throw;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[AuthHeaderHandler] Problème de connexion réseau ou HTTP : {ex.Message}");
                // Optionally: throw a custom exception to signal network issues gracefully
                throw new NetworkException("Impossible de contacter le serveur. Vérifiez votre connexion.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthHeaderHandler] Erreur inattendue : {ex}");
                // Optionally: throw a custom exception to handle unexpected errors at a higher level
                throw new AuthHeaderHandlerException("Une erreur interne s'est produite lors de la préparation de la requête.", ex);
            }
        }
    }

    /// <summary>
    /// Exception personnalisée pour les erreurs réseaux.
    /// </summary>
    public class NetworkException : Exception
    {
        public NetworkException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception personnalisée pour identifier les erreurs liées à AuthHeaderHandler.
    /// </summary>
    public class AuthHeaderHandlerException : Exception
    {
        public AuthHeaderHandlerException(string message, Exception innerException) : base(message, innerException) { }
    }
}