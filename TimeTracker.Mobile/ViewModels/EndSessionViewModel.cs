using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Resources.Strings;
using TimeTracker.Mobile.Services.Interfaces;

namespace TimeTracker.Mobile.ViewModels
{
    public partial class EndSessionViewModel : BaseViewModel
    {
        private readonly IMobileTimeEntryService _timeEntryService;
        private readonly IApiClientService _apiClient;
        private readonly IGeolocationService _geoService;
        private readonly INavigationService _nav;
        private readonly IDialogService _dialogs;
        private readonly IAuthService _authService;
        private CancellationTokenSource? _pauseReminderCts;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private PausePeriodDto? currentPause;

        private bool isPaused;
        public bool IsPaused
        {
            get => isPaused;
            set => SetProperty(ref isPaused, value);
        }

        public ICommand PauseCommand { get; }
        public ICommand ResumeCommand { get; }
        public ICommand EndCommand { get; }

        public EndSessionViewModel(
            IMobileTimeEntryService timeEntryService,
            IGeolocationService geoService,
            INavigationService navigationService,
            IDialogService dialogService,
            IApiClientService apiClientService,
            IAuthService authService)
        {
            _timeEntryService = timeEntryService;
            _geoService = geoService;
            _nav = navigationService;
            _dialogs = dialogService;
            _apiClient = apiClientService;
            _authService = authService;

            PauseCommand = new AsyncRelayCommand(OnPauseAsync);
            ResumeCommand = new AsyncRelayCommand(OnResumeAsync);
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
                OnPropertyChanged(nameof(StartTimeLocal));
                OnPropertyChanged(nameof(EndTimeLocal));
            }
        }

        // Affichage horodaté en local
        public string InProgressSessionInfo =>
            InProgressSession != null
                ? $"{InProgressSession.SessionType} – {AppResources.EndSession_StartedAt} {StartTimeLocal}"
                : AppResources.EndSession_NoSessionInProgress;

        public string StartTimeLocal
        {
            get
            {
                var dt = InProgressSession?.StartTime;
                if (dt is DateTime d)
                    return d.ToLocalTime().ToString("g");
                return string.Empty;
            }
        }

        public string EndTimeLocal
        {
            get
            {
                var dt = InProgressSession?.EndTime;
                if (dt is DateTime d)
                    return d.ToLocalTime().ToString("g");
                return string.Empty;
            }
        }

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
            currentPause = InProgressSession?.Pauses?.LastOrDefault(p => !p.End.HasValue);
            IsPaused = currentPause != null;
        }

        private async Task OnPauseAsync()
        {
            if (IsPaused) return;

            var inProgress = _timeEntryService.InProgressSession;
            if (inProgress == null)
            {
                await _dialogs.ShowAlertAsync("Info", "Aucune session en cours détectée.", "OK");
                return;
            }

            var open = FindOpenPause(inProgress);
            if (open != null)
            {
                IsPaused = true;
                currentPause = open;
                StartPauseReminders();
                await _dialogs.ShowSuccessAsync("Pause déjà active", "Pause");
                return;
            }

            currentPause = new PausePeriodDto { Start = DateTime.Now };
            inProgress.Pauses ??= new System.Collections.Generic.List<PausePeriodDto>();
            inProgress.Pauses.Add(currentPause);

            await _timeEntryService.StartSessionAsync(inProgress);

            IsPaused = true;
            StartPauseReminders();

            await _dialogs.ShowSuccessAsync("Pause démarrée", "Pause");
        }

        private async Task OnResumeAsync()
        {
            if (!IsPaused)
                return;

            var inProgress = _timeEntryService.InProgressSession;
            if (inProgress == null)
            {
                IsPaused = false;
                currentPause = null;
                StopPauseReminders();
                await _dialogs.ShowAlertAsync("Info", "Aucune session en cours détectée.", "OK");
                return;
            }

            currentPause ??= FindOpenPause(inProgress);
            if (currentPause == null)
            {
                IsPaused = false;
                StopPauseReminders();
                await _dialogs.ShowAlertAsync("Info", "Aucune pause ouverte à reprendre.", "OK");
                return;
            }

            currentPause.End = DateTime.Now;
            await _timeEntryService.StartSessionAsync(inProgress);

            currentPause = null;
            IsPaused = false;
            StopPauseReminders();

            await _dialogs.ShowSuccessAsync("Pause terminée", "Reprise");
        }

        private PausePeriodDto? FindOpenPause(TimeEntryDto s)
            => s.Pauses?.LastOrDefault(p => !p.End.HasValue);

        private void StartPauseReminders()
        {
            StopPauseReminders();
            _pauseReminderCts = new CancellationTokenSource();
            var token = _pauseReminderCts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested && IsPaused)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(15), token);
                        if (!token.IsCancellationRequested && IsPaused)
                        {
                            await _dialogs.ShowSuccessAsync(
                                "Vous êtes toujours en pause. Pensez à reprendre si nécessaire.",
                                "Rappel");
                        }
                    }
                }
                catch (TaskCanceledException) { }
            }, token);
        }

        private void StopPauseReminders()
        {
            try { _pauseReminderCts?.Cancel(); }
            catch { }
            finally { _pauseReminderCts?.Dispose(); _pauseReminderCts = null; }
        }

        private async Task OnEndSessionAsync()
        {
            if (IsBusy) return; // Ajout : empêche double clic/nouvel appui
            IsBusy = true;
            try
            {
                var session = InProgressSession ?? _timeEntryService.InProgressSession;
                if (session == null)
                {
                    await _dialogs.ShowAlertAsync(
                        AppResources.EndSession_Error_Title,
                        AppResources.EndSession_Error_NoSession,
                        AppResources.EndSession_Error_OK);
                    return;
                }

                if (IsPaused)
                {
                    await _dialogs.ShowAlertAsync("Pause en cours",
                        "Termine ou annule la pause avant de clôturer la session.",
                        "OK");
                    return;
                }

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

                double? travelDurationHours = null;
                if (session.IncludesTravelTime)
                {
                    int h = 0, m = 0;
                    var okH = int.TryParse(TravelHours, out h);
                    var okM = int.TryParse(TravelMinutes, out m);
                    if (okH || okM)
                    {
                        if (h < 0) h = 0;
                        if (m < 0) m = 0;
                        if (m > 59) m = 59;
                        travelDurationHours = h + (m / 60.0);
                    }
                }

                session.TravelDurationHours = travelDurationHours;
                session.EndTime = DateTime.Now;
                session.EndLatitude = lat;
                session.EndLongitude = lon;
                session.EndAddress = endAddress;
                session.DinnerPaid = selectedDinnerPaidBy;

                // Set Username if needed
                if (string.IsNullOrWhiteSpace(session.Username) && _authService.CurrentUser != null)
                {
                    session.Username = _authService.CurrentUser.UserName ?? string.Empty;
                }

                await _timeEntryService.StartSessionAsync(session);

                // --- Correction ici : Pour la création, il faut forcer Id=0 et POST ---
                if (session.Id > 0)
                {
                    session.Id = 0;
                }

                var createRes = await _apiClient.CreateTimeEntryAsync(session);

                if (!createRes.IsSuccess)
                {
                    // Always log payload in case of failure
                    try
                    {
                        var payload = JsonSerializer.Serialize(session, JsonOpts);
                        System.Diagnostics.Debug.WriteLine(
                            $"[CreateEntry] FAILED\nPayload: {payload}\nError: {createRes.Error ?? "<none>"}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[CreateEntry] Payload logging error: {ex.Message}");
                    }

                    await _dialogs.ShowErrorAsync(
                        createRes.Error +
                        "\n\n(Détail technique envoyé dans les logs développeur, voir Debug Output.)"
                    );
                    return;
                }

                await _timeEntryService.StartSessionAsync(session);

                try
                {
                    await _timeEntryService.EndAndSaveCurrentSessionAsync();
                    await _nav.GoToHomePageAsync();
                }
                catch (Exception ex)
                {
                    try
                    {
                        var payload = JsonSerializer.Serialize(session, JsonOpts);
                        System.Diagnostics.Debug.WriteLine($"[EndSession] Save exception: {ex.Message}\nPayload: {payload}");
                    }
                    catch { }
                    await _dialogs.ShowErrorAsync("Impossible d'enregistrer la session.", "Erreur");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}