using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Resources.Strings; // Ajout pour i18n

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
                ErrorMessage = AppResources.TimeEntries_NotAuthenticated;
                return;
            }

            var result = await _apiClient.GetTimeEntriesAsync(user.Id);
            if (result.IsSuccess && result.Value is not null)
            {
                timeEntries.Clear();
                foreach (var entry in result.Value.Where(e => e.EndTime != null))
                    timeEntries.Add(entry);
            }
            else
            {
                ErrorMessage = result.Error ?? AppResources.TimeEntries_FailedToLoad;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}