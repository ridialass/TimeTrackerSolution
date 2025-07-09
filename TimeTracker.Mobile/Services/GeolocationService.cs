using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using CommunityToolkit.Maui.Alerts;

namespace TimeTracker.Mobile.Services;

public class GeolocationService : IGeolocationService
{
    public async Task<Location?> GetCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
                return null;

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            return await Geolocation.GetLocationAsync(request);
        }
        catch (Exception ex)
        {
            await Toast.Make($"Erreur localisation : {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            return null;
        }
    }

    public async Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            // Timeout "maison" : si MAUI ne supporte pas CancellationToken ici
            var geocodeTask = Geocoding.GetPlacemarksAsync(latitude, longitude);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));
            var finishedTask = await Task.WhenAny(geocodeTask, timeoutTask);

            if (finishedTask == timeoutTask)
            {
                await Toast.Make("La géolocalisation a mis trop de temps.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
                return "Localisation trop longue ou indisponible";
            }

            var placemarks = await geocodeTask;
            var placemark = placemarks?.FirstOrDefault();

            if (placemark != null)
            {
                return $"{placemark.Thoroughfare} {placemark.SubThoroughfare}, " +
                       $"{placemark.Locality}, {placemark.AdminArea}, {placemark.CountryName}";
            }
        }
        catch (Exception ex)
        {
            await Toast.Make($"Erreur géolocalisation : {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        }

        return "Localisation inconnue";
    }
}