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
using TimeTracker.Mobile.Resources.Strings; // Ajout pour i18n

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

    private TimeEntryDto? _inProgressSession;
    public TimeEntryDto? InProgressSession
    {
        get => _inProgressSession;
        set => SetProperty(ref _inProgressSession, value);
    }

    public string InProgressSessionInfo =>
        InProgressSession != null
            ? $"{InProgressSession.SessionType} – {AppResources.EndSession_StartedAt} {InProgressSession.StartTime:g}"
            : AppResources.EndSession_NoSessionInProgress;

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

    public async Task ReloadSessionAsync()
    {
        // Recharge la session depuis le stockage persistant
        await _timeEntryService.LoadInProgressSessionAsync();
        InProgressSession = _timeEntryService.InProgressSession;
        OnPropertyChanged(nameof(InProgressSessionInfo));
        OnPropertyChanged(nameof(InProgressSessionIncludesTravel));
    }

    private async Task OnEndSessionAsync()
    {
        var session = InProgressSession;
        if (session == null)
        {
            await Application.Current.MainPage.DisplayAlert(
                AppResources.EndSession_Error_Title,
                AppResources.EndSession_Error_NoSession,
                AppResources.EndSession_Error_OK);
            return;
        }

        var loc = await _geoService.GetCurrentLocationAsync();
        string endAddress = AppResources.EndSession_LocationUnavailable;
        double lat = 0, lon = 0;

        if (loc != null)
        {
            lat = loc.Latitude;
            lon = loc.Longitude;
            endAddress = await _geoService.GetAddressFromCoordinatesAsync(lat, lon);
        }

        // --- Correction ici ---
        double? travelDurationHours = null;
        if (session.IncludesTravelTime
            && int.TryParse(travelHours, out var h)
            && int.TryParse(travelMinutes, out var m))
        {
            travelDurationHours = h + m / 60.0;
        }
        session.TravelDurationHours = travelDurationHours;
        // --- Fin correction ---

        session.EndTime = DateTime.Now;
        session.EndLatitude = lat;
        session.EndLongitude = lon;
        session.EndAddress = endAddress;
        session.DinnerPaid = selectedDinnerPaidBy;

        await _timeEntryService.EndAndSaveCurrentSessionAsync();

        await Shell.Current.GoToAsync("///HomePage");
    }
}