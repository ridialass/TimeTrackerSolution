
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace TimeTracker.Mobile.Services
{
    public class LocationService
    {
        public async Task<Location?> GetCurrentLocationAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    Debug.WriteLine("Location permission denied.");
                    return null;
                }

                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request);
                return location;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocationService] Failed to get location: {ex.Message}");
                return null;
            }
        }
    }
}
