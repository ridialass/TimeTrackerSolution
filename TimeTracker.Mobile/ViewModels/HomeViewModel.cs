using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    public ObservableCollection<WorkSessionType> SessionTypes { get; }
        = new((WorkSessionType[])Enum.GetValues(typeof(WorkSessionType)));

    public ObservableCollection<DinnerPaidBy> DinnerPaidByOptions { get; }
        = new((DinnerPaidBy[])Enum.GetValues(typeof(DinnerPaidBy)));

    private WorkSessionType selectedSessionType;
    public WorkSessionType SelectedSessionType
    {
        get => selectedSessionType;
        set => SetProperty(ref selectedSessionType, value);
    }

    
    private bool includesTravelTime;
    public bool IncludesTravelTime
    {
        get => includesTravelTime;
        set => SetProperty(ref includesTravelTime, value);
    }

  
    private string travelHours = string.Empty;
    public string TravelHours
    {
        get => travelHours;
        set => SetProperty(ref travelHours, value);
    }

    
    private string travelMinutes = string.Empty;
    public string TravelMinutes
    {
        get => travelMinutes;
        set => SetProperty(ref travelMinutes, value);
    }

   
    private DinnerPaidBy selectedDinnerPaidBy = DinnerPaidBy.None;
    public DinnerPaidBy SelectedDinnerPaidBy
    {
        get => selectedDinnerPaidBy;
        set => SetProperty(ref selectedDinnerPaidBy, value);
    }

    public ICommand ClockInCommand { get; }
    public ICommand ClockOutCommand { get; }
    public ICommand GoToHistoryCommand { get; }

    private TimeEntryDto? _currentEntry;
    private readonly IAuthService _authService;
    private readonly IMobileTimeEntryService _timeEntryService;
    private readonly IGeolocationService _geoService;

    public HomeViewModel(
        IAuthService authService,
        IMobileTimeEntryService timeEntryService,
        IGeolocationService geoService)
    {
        _authService = authService;
        _timeEntryService = timeEntryService;
        _geoService = geoService;

        ClockInCommand = new Command(async () => await OnClockInAsync());
        ClockOutCommand = new Command(async () => await OnClockOutAsync());
        GoToHistoryCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(TimeEntriesPage)));
    }

    public bool IsCurrentUserAdmin =>
        _authService.CurrentUser?.Role == "Admin";

    private async Task OnClockInAsync()
    {
        var loc = await _geoService.GetCurrentLocationAsync();
        var addr = "Location unavailable";
        double lat = 0, lon = 0;

        if (loc != null)
        {
            lat = loc.Latitude;
            lon = loc.Longitude;
            addr = await _geoService.GetAddressFromCoordinatesAsync(lat, lon);
        }

        _currentEntry = new TimeEntryDto
        {
            UserId = _authService.CurrentUser!.Id,
            Username = _authService.CurrentUser.UserName!,
            SessionType = selectedSessionType,
            StartTime = DateTime.UtcNow,
            StartLatitude = lat,
            StartLongitude = lon,
            StartAddress = addr,
            IncludesTravelTime = includesTravelTime,
            DinnerPaid = DinnerPaidBy.None,
            Location = addr
        };

        travelHours = "";
        travelMinutes = "";
    }

    private async Task OnClockOutAsync()
    {
        if (_currentEntry is null) return;

        var loc = await _geoService.GetCurrentLocationAsync();
        var endAddr = "Location unavailable";
        double lat = 0, lon = 0;

        if (loc != null)
        {
            lat = loc.Latitude;
            lon = loc.Longitude;
            endAddr = await _geoService.GetAddressFromCoordinatesAsync(lat, lon);
        }

        TimeSpan? travelSpan = null;
        if (includesTravelTime
            && int.TryParse(travelHours, out var h)
            && int.TryParse(travelMinutes, out var m))
        {
            travelSpan = TimeSpan.FromHours(h + m / 60.0);
        }

        var completed = new TimeEntryDto
        {
            Id = _currentEntry.Id,
            StartTime = _currentEntry.StartTime,
            EndTime = DateTime.UtcNow,
            StartLatitude = _currentEntry.StartLatitude,
            StartLongitude = _currentEntry.StartLongitude,
            StartAddress = _currentEntry.StartAddress,
            EndLatitude = lat,
            EndLongitude = lon,
            EndAddress = endAddr,
            SessionType = _currentEntry.SessionType,
            IncludesTravelTime = _currentEntry.IncludesTravelTime,
            TravelDurationHours = _currentEntry.TravelDurationHours,
            DinnerPaid = selectedDinnerPaidBy,
            Location = _currentEntry.Location,
            UserId = _currentEntry.UserId,
            Username = _currentEntry.Username
        };

        if (travelSpan.HasValue)
        {
            Console.WriteLine($"Durée estimée du trajet : {travelSpan.Value}");
        }

        try
        {
            await _timeEntryService.CreateTimeEntryAsync(completed);
        }
        catch
        {
            // TODO : afficher un message
        }
        finally
        {
            _currentEntry = null;
        }
    }
}
