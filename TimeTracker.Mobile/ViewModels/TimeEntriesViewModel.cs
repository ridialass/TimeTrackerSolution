using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Services.Interfaces;
using TimeTracker.Mobile.Resources.Strings;

namespace TimeTracker.Mobile.ViewModels;

public partial class TimeEntriesViewModel : BaseViewModel
{
    private readonly IMobileTimeEntryService _timeEntryService;
    private readonly IAuthService _authService;

    private ObservableCollection<TimeEntryDto> timeEntries = new();
    public ObservableCollection<TimeEntryDto> TimeEntries
    {
        get => timeEntries;
        set => SetProperty(ref timeEntries, value);
    }

    public TimeEntriesViewModel(IMobileTimeEntryService timeEntryService, IAuthService authService)
    {
        _timeEntryService = timeEntryService;
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

            var entries = await _timeEntryService.GetTimeEntriesAsync(user.Id);
            timeEntries.Clear();
            foreach (var entry in entries.Where(e => e.EndTime != null))
                timeEntries.Add(entry);
        }
        finally
        {
            IsBusy = false;
        }
    }
}