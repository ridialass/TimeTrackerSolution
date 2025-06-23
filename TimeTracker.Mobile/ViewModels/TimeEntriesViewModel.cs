using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Services;

namespace TimeTracker.Mobile.ViewModels;

public partial class TimeEntriesViewModel : BaseViewModel
{
    private readonly IApiClientService _apiClient;
    private readonly IAuthService _authService;

    private ObservableCollection<TimeEntryDto> timeEntries = new();
    public ObservableCollection<TimeEntryDto> TimeEntries
    {
        get => timeEntries;
        set => SetProperty(ref timeEntries, value);
    }

    // 🔥 Removed duplicated errorMessage — already inherited

    public TimeEntriesViewModel(IApiClientService apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    [RelayCommand]
    public async Task LoadTimeEntriesAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var user = _authService.CurrentUser;
            if (user == null)
            {
                ErrorMessage = "Not authenticated.";
                return;
            }

            var result = await _apiClient.GetTimeEntriesAsync(user.Id);
            if (result.IsSuccess && result.Value is not null)
            {
                timeEntries.Clear();
                foreach (var entry in result.Value)
                    timeEntries.Add(entry);
            }
            else
            {
                ErrorMessage = result.Error ?? "Failed to load time entries.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
