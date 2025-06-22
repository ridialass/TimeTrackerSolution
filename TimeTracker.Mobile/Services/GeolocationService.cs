using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

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
            Console.WriteLine($"[GeolocationService] Failed to get location: {ex.Message}");
            return null;
        }
    }

    public async Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            var placemarks = await Geocoding.GetPlacemarksAsync(latitude, longitude);
            var placemark = placemarks?.FirstOrDefault();

            if (placemark != null)
            {
                return $"{placemark.Thoroughfare} {placemark.SubThoroughfare}, " +
                       $"{placemark.Locality}, {placemark.AdminArea}, {placemark.CountryName}";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GeolocationService] Reverse geocoding failed: {ex.Message}");
        }

        return "Unknown Location";
    }
}
