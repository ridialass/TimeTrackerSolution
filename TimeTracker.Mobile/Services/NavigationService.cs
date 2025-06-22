using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace TimeTracker.Mobile.Services;

public class NavigationService : INavigationService
{
    public Task GoToHomePageAsync() =>
        Shell.Current.GoToAsync("//HomePage"); // ✅ existing route

    public Task GoToLoginPageAsync() =>
        Shell.Current.GoToAsync("//LoginPage");

    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        if (parameters != null)
            return Shell.Current.GoToAsync(route, parameters);
        else
            return Shell.Current.GoToAsync(route);
    }
}