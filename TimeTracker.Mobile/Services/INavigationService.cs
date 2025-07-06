using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace TimeTracker.Mobile.Services;

public interface INavigationService
{
    Task GoToLoginPageAsync();
    Task GoToHomePageAsync();
    Task GoToAdminDashboardPageAsync();
    Task GoToStartSessionPageAsync();
    Task GoToEndSessionPageAsync();
    Task GoToTimeEntriesPageAsync();
    Task GoBackAsync();
}

