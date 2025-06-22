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

public partial class EndSessionViewModel : BaseViewModel
{
    private readonly IMobileTimeEntryService _timeEntryService;
    private readonly IGeolocationService _geoService;

    public ICommand EndCommand { get; }

    public EndSessionViewModel(
        IMobileTimeEntryService timeEntryService,
        IGeolocationService geoService)
    {
        _timeEntryService = timeEntryService;
        _geoService = geoService;

        EndCommand = new Command(async () => await OnEndSessionAsync());
    }

    // Properties exposed for binding

    public TimeEntryDto? InProgressSession => _timeEntryService.InProgressSession;

    public string InProgressSessionInfo =>
        InProgressSession != null
            ? $"{InProgressSession.SessionType} – démarrée à {InProgressSession.StartTime:g}"
            : "Aucune session en cours";

    public bool InProgressSessionIncludesTravel =>
        InProgressSession?.IncludesTravelTime == true;

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

    public ObservableCollection<DinnerPaidBy> DinnerPaidByOptions { get; }
        = new(Enum.GetValues<DinnerPaidBy>());

    private DinnerPaidBy selectedDinnerPaidBy = DinnerPaidBy.None;
    public DinnerPaidBy SelectedDinnerPaidBy
    {
        get => selectedDinnerPaidBy;
        set => SetProperty(ref selectedDinnerPaidBy, value);
    }

    private async Task OnEndSessionAsync()
    {
        var session = InProgressSession;
        if (session == null)
            return;

        var loc = await _geoService.GetCurrentLocationAsync();
        string endAddress = "Localisation indisponible";
        double lat = 0, lon = 0;

        if (loc != null)
        {
            lat = loc.Latitude;
            lon = loc.Longitude;
            endAddress = await _geoService.GetAddressFromCoordinatesAsync(lat, lon);
        }

        if (session.IncludesTravelTime
            && int.TryParse(travelHours, out var h)
            && int.TryParse(travelMinutes, out var m))
        {
            session.TravelDurationHours = h + (m / 60.0);
        }
        else
        {
            session.TravelDurationHours = null;
        }

        session.EndTime = DateTime.Now;
        session.EndLatitude = lat;
        session.EndLongitude = lon;
        session.EndAddress = endAddress;
        session.DinnerPaid = selectedDinnerPaidBy;

        await _timeEntryService.EndAndSaveCurrentSessionAsync();

        await Shell.Current.GoToAsync(nameof(HomePage));
    }
}
