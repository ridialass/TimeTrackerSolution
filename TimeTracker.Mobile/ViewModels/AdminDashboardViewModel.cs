using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Services;

namespace TimeTracker.Mobile.ViewModels;

public partial class AdminDashboardViewModel : BaseViewModel
{
    private readonly IApiClientService _apiClient;
    private readonly IAuthService _authService;
    private readonly INavigationService _navigation;

    private ObservableCollection<EmployeeDto> users = new();
    public ObservableCollection<EmployeeDto> Users
    {
        get => users;
        set => SetProperty(ref users, value);
    }

    // 🔁 REMOVED duplicate errorMessage: it's already in BaseViewModel

    public AdminDashboardViewModel(
        IApiClientService apiClient,
        IAuthService authService,
        INavigationService navigation)
    {
        _apiClient = apiClient;
        _authService = authService;
        _navigation = navigation;
    }

    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _apiClient.GetEmployeesAsync();
            if (result.IsSuccess && result.Value is not null)
            {
                users.Clear();
                foreach (var u in result.Value)
                    users.Add(u);
            }
            else
            {
                ErrorMessage = result.Error ?? "Failed to load users.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
