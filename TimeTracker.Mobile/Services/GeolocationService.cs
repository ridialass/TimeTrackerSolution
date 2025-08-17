#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using TimeTracker.Mobile.Services.Interfaces;

namespace TimeTracker.Mobile.Services
{
    public class GeolocationService : IGeolocationService
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

        public async Task<Location?> GetCurrentLocationAsync()
        {
            try
            {
                // 1) Permissions
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    await ShowToastAsync("Permission de localisation refusée.");
                    return null;
                }

                // 2) Requête avec timeout + précision raisonnable
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, DefaultTimeout);
                using var cts = new CancellationTokenSource(DefaultTimeout);

                Location? location = null;
                try
                {
                    // Cette surcharge avec CancellationToken existe bien en MAUI
                    location = await Geolocation.GetLocationAsync(request, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Timeout/annulation : on tentera la dernière position connue
                }

                // 3) Fallback : dernière position connue
                if (location is null)
                    location = await Geolocation.GetLastKnownLocationAsync();

                if (location is null)
                    await ShowToastAsync("Impossible d'obtenir la position actuelle.");

                return location;
            }
            catch (FeatureNotEnabledException)
            {
                await ShowToastAsync("La localisation est désactivée. Active-la dans les réglages.");
                return null;
            }
            catch (FeatureNotSupportedException)
            {
                await ShowToastAsync("La localisation n'est pas supportée sur cet appareil.");
                return null;
            }
            catch (PermissionException)
            {
                await ShowToastAsync("La permission de localisation est requise.");
                return null;
            }
            catch (Exception ex)
            {
                await ShowToastAsync($"Erreur localisation : {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude)
        {
            try
            {
                // Pas d'overload avec CancellationToken → on fait un timeout maison
                var geocodeTask = Geocoding.GetPlacemarksAsync(latitude, longitude);
                var timeoutTask = Task.Delay(DefaultTimeout);

                var finished = await Task.WhenAny(geocodeTask, timeoutTask);
                if (finished != geocodeTask)
                {
                    await ShowToastAsync("La géolocalisation a mis trop de temps.");
                    return "Localisation trop longue ou indisponible";
                }

                var placemarks = await geocodeTask; // safe, la tâche est terminée
                var p = placemarks?.FirstOrDefault();
                if (p is null)
                    return "Adresse introuvable";

                string street = JoinNonEmpty(" ", p.Thoroughfare, p.SubThoroughfare);
                string city = FirstNonEmpty(p.Locality, p.SubAdminArea);
                string region = FirstNonEmpty(p.AdminArea, p.PostalCode);
                string country = FirstNonEmpty(p.CountryName, p.CountryCode);

                var parts = new[] { street, city, region, country }
                    .Where(s => !string.IsNullOrWhiteSpace(s));
                var address = string.Join(", ", parts);

                return string.IsNullOrWhiteSpace(address) ? "Localisation inconnue" : address;
            }
            catch (Exception ex)
            {
                await ShowToastAsync($"Erreur géocodage : {ex.Message}");
                return "Localisation inconnue";
            }
        }

        // ----------------- Helpers -----------------

        private static string FirstNonEmpty(params string?[] values) =>
            values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty;

        private static string JoinNonEmpty(string sep, params string?[] values) =>
            string.Join(sep, values.Where(v => !string.IsNullOrWhiteSpace(v)));

        private static Task ShowToastAsync(string message) =>
            MainThread.InvokeOnMainThreadAsync(() => Toast.Make(message, ToastDuration.Long).Show());
    }
}
