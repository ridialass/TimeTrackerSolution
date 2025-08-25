using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services.Interfaces;
using TimeTracker.Mobile.Resources.Strings;

namespace TimeTracker.Mobile.ViewModels;

public partial class StartSessionViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IMobileTimeEntryService _timeEntryService;
    private readonly IGeolocationService _geoService;
    private readonly IClockService _clockService;

    public ObservableCollection<WorkSessionType> SessionTypes { get; }
        = new(Enum.GetValues<WorkSessionType>());

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

    private DateTime? startTimeUtc;
    public DateTime? StartTimeUtc
    {
        get => startTimeUtc;
        set
        {
            SetProperty(ref startTimeUtc, value);
            OnPropertyChanged(nameof(StartTimeLocal));
        }
    }

    // Propriété d'affichage pour l'heure locale
    public string StartTimeLocal =>
        StartTimeUtc.HasValue
            ? StartTimeUtc.Value.ToLocalTime().ToString("g") // format court local
            : string.Empty;

    public ICommand StartCommand { get; }

    public StartSessionViewModel(
        IAuthService authService,
        IMobileTimeEntryService timeEntryService,
        IGeolocationService geoService,
        IClockService clockService)
    {
        _authService = authService;
        _timeEntryService = timeEntryService;
        _geoService = geoService;
        _clockService = clockService;

        StartCommand = new Command(async () => await OnStartSessionAsync(),
            () => !IsBusy);
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IsBusy))
                ((Command)StartCommand).ChangeCanExecute();
        };
    }

    private async Task OnStartSessionAsync()
    {
        IsBusy = true;
        try
        {
            var loc = await _geoService.GetCurrentLocationAsync();
            string address = AppResources.StartSession_LocationUnavailable;
            double lat = 0, lon = 0;

            if (loc != null)
            {
                lat = loc.Latitude;
                lon = loc.Longitude;
                address = await _geoService.GetAddressFromCoordinatesAsync(lat, lon);
            }

            var user = _authService.CurrentUser;
            if (user == null)
            {
                await Shell.Current.DisplayAlert(
                    AppResources.StartSession_Error_Title,
                    AppResources.StartSession_Error_NoUser,
                    AppResources.StartSession_Error_OK);
                return;
            }

            var utcNow = _clockService.UtcNow; // Utilise l'heure UTC sécurisée
            StartTimeUtc = utcNow;

            var dto = new TimeEntryDto
            {
                UserId = user.Id,
                Username = user.UserName!,
                SessionType = selectedSessionType,
                StartTime = utcNow,
                IncludesTravelTime = includesTravelTime,
                StartLatitude = lat,
                StartLongitude = lon,
                StartAddress = address,
                DinnerPaid = DinnerPaidBy.None,
                Location = address
            };

            try
            {
                await _timeEntryService.StartSessionAsync(dto);
            }
            catch
            {
                await Shell.Current.DisplayAlert(
                    AppResources.StartSession_Error_Title,
                    AppResources.StartSession_Error_CannotStart,
                    AppResources.StartSession_Error_OK);
                return;
            }

            await Shell.Current.GoToAsync(nameof(TimeTracker.Mobile.Views.EndSessionPage));
        }
        finally
        {
            IsBusy = false;
        }
    }
}