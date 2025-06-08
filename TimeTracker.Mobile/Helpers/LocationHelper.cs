using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;

namespace TimeTracker.Mobile.Helpers
{
    public static class LocationHelper
    {
        public static async Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude)
        {
            try
            {
                // Récupère les placemarks à partir des coordonnées
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
                Console.WriteLine($"Reverse geocoding failed: {ex.Message}");
            }

            return "Unknown Location";
        }
    }
}
