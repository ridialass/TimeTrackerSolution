using System.Threading.Tasks;

namespace TimeTracker.Mobile.Services;

/// <summary>
/// Abstract navigation for easier testing and decoupling.
/// </summary>
public interface INavigationService
{
    Task GoToHomePageAsync(); // 🔁 rename to reflect real destination
    Task GoToLoginPageAsync();
    Task GoToAsync(string route, IDictionary<string, object>? parameters = null);

}