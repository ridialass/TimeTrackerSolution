using System.Threading.Tasks;

namespace TimeTracker.Mobile.Services;

public interface IGeolocationService
{
    Task<Location?> GetCurrentLocationAsync();
    Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude);
}
