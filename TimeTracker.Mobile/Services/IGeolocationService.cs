#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace TimeTracker.Mobile.Services
{
    /// <summary>
    /// Abstraction de la géolocalisation pour l'app mobile (MAUI).
    /// Implémentation recommandée : <see cref="GeolocationService"/>.
    /// </summary>
    public interface IGeolocationService
    {
        /// <summary>Retourne la position courante ou <c>null</c> si indisponible.</summary>
        Task<Location?> GetCurrentLocationAsync();

        /// <summary>Résout des coordonnées en adresse lisible (ou un libellé par défaut).</summary>
        Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude);
    }
}
