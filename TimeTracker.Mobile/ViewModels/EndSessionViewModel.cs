using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Resources.Strings;

namespace TimeTracker.Mobile.ViewModels
{
    public partial class EndSessionViewModel : BaseViewModel
    {
        private readonly IMobileTimeEntryService _timeEntryService;
        private readonly IGeolocationService _geoService;
        private readonly INavigationService _nav;
        private readonly IDialogService _dialogs;

        public ICommand EndCommand { get; }

        public EndSessionViewModel(
            IMobileTimeEntryService timeEntryService,
            IGeolocationService geoService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _timeEntryService = timeEntryService;
            _geoService = geoService;
            _nav = navigationService;
            _dialogs = dialogService;

            EndCommand = new AsyncRelayCommand(OnEndSessionAsync);
        }

        private TimeEntryDto? _inProgressSession;
        public TimeEntryDto? InProgressSession
        {
            get => _inProgressSession;
            set
            {
                SetProperty(ref _inProgressSession, value);
                OnPropertyChanged(nameof(InProgressSessionInfo));
                OnPropertyChanged(nameof(InProgressSessionIncludesTravel));
            }
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
            await _timeEntryService.LoadInProgressSessionAsync();
            InProgressSession = _timeEntryService.InProgressSession;
        }

        private async Task OnEndSessionAsync()
        {
            var session = InProgressSession;
            if (session == null)
            {
                await _dialogs.ShowAlertAsync(
                    AppResources.EndSession_Error_Title,
                    AppResources.EndSession_Error_NoSession,
                    AppResources.EndSession_Error_OK);
                return;
            }

            // Localisation de fin (best effort)
            string endAddress = AppResources.EndSession_LocationUnavailable;
            double lat = 0, lon = 0;

            try
            {
                var loc = await _geoService.GetCurrentLocationAsync();
                if (loc != null)
                {
                    lat = loc.Latitude;
                    lon = loc.Longitude;
                    endAddress = await _geoService.GetAddressFromCoordinatesAsync(lat, lon);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EndSession] Geolocation error: {ex.Message}");
            }

            // ---- Temps de trajet : assigne TravelDurationHours (TravelTimeEstimate est read-only) ----
            double? travelDurationHours = null;
            if (session.IncludesTravelTime)
            {
                int h = 0, m = 0;
                bool okH = int.TryParse(TravelHours, out h);
                bool okM = int.TryParse(TravelMinutes, out m);
                if (okH || okM)
                {
                    if (h < 0) h = 0;
                    if (m < 0) m = 0;
                    if (m > 59) m = 59;
                    travelDurationHours = h + (m / 60.0);
                }
            }
            session.TravelDurationHours = travelDurationHours;
            // -------------------------------------------------------------------------------------------

            session.EndTime = DateTime.Now;
            session.EndLatitude = lat;
            session.EndLongitude = lon;
            session.EndAddress = endAddress;
            session.DinnerPaid = selectedDinnerPaidBy;

            try
            {
                await _timeEntryService.EndAndSaveCurrentSessionAsync();
                await _nav.GoToHomePageAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EndSession] Save error: {ex.Message}");
                // Fallback si la ressource n’existe pas
                await _dialogs.ShowErrorAsync("Impossible d'enregistrer la session.", "Erreur");
            }
        }
    }
}
