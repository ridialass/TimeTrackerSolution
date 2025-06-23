using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimeTracker.Mobile.Services
{
    public interface INavigationService
    {
        Task GoToLoginPageAsync();
        Task GoToHomePageAsync();
        Task GoToAdminDashboardPageAsync();
        Task GoToAsync(string route, IDictionary<string, object>? parameters = null);
    }
}
