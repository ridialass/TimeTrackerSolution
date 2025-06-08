using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;                     // pour Command et Shell
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.Helpers;
using static TimeTracker.Models.Enum;
using TimeTracker.Views;
using TimeTracker;

namespace TimeTracker.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        // ════ SOURCES DÉNUMÉRÉES ════
        public ObservableCollection<WorkSessionType> SessionTypes { get; }
            = new ObservableCollection<WorkSessionType>(
                (WorkSessionType[])System.Enum.GetValues(typeof(WorkSessionType)));

        public ObservableCollection<DinnerPaidBy> DinnerPaidByOptions { get; }
            = new ObservableCollection<DinnerPaidBy>(
                (DinnerPaidBy[])System.Enum.GetValues(typeof(DinnerPaidBy)));

        // ════ PROPRIÉTÉS BINDÉES ════

        private WorkSessionType _selectedSessionType;
        public WorkSessionType SelectedSessionType
        {
            get => _selectedSessionType;
            set => SetProperty(ref _selectedSessionType, value);
        }

        private bool _includesTravelTime;
        public bool IncludesTravelTime
        {
            get => _includesTravelTime;
            set => SetProperty(ref _includesTravelTime, value);
        }

        // ⚠️ Nouveaux champs pour « Heures » et « Minutes » de trajet
        private string _travelHours;
        public string TravelHours
        {
            get => _travelHours;
            set => SetProperty(ref _travelHours, value);
        }

        private string _travelMinutes;
        public string TravelMinutes
        {
            get => _travelMinutes;
            set => SetProperty(ref _travelMinutes, value);
        }

        private DinnerPaidBy _selectedDinnerPaidBy = DinnerPaidBy.None;
        public DinnerPaidBy SelectedDinnerPaidBy
        {
            get => _selectedDinnerPaidBy;
            set => SetProperty(ref _selectedDinnerPaidBy, value);
        }

        // ════ COMMANDES ════
        public ICommand ClockInCommand { get; }
        public ICommand ClockOutCommand { get; }
        public ICommand GoToHistoryCommand { get; }

        // ════ ÉTAT INTERNE ════
        private WorkSession currentSession;
        private readonly LocationService _locationService = new LocationService();
        private readonly SessionService _sessionService = App.SessionService;

        public HomeViewModel()
        {
            ClockInCommand = new Command(OnClockIn);
            ClockOutCommand = new Command(OnClockOut);
            GoToHistoryCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(SessionHistoryPage)));
        }

        /// <summary>
        /// Indique si l'utilisateur actuellement connecté a le rôle Admin.
        /// </summary>
        public bool IsCurrentUserAdmin =>
            App.AuthService.CurrentUser?.Role == UserRole.Admin;

        private async void OnClockIn()
        {
            // 1) Récupérer la position GPS
            var location = await _locationService.GetCurrentLocationAsync();

            string startAddress = "Location unavailable";
            double lat = 0, lon = 0;

            if (location != null)
            {
                lat = location.Latitude;
                lon = location.Longitude;
                startAddress = await LocationHelper.GetAddressFromCoordinatesAsync(lat, lon);
            }

            // 2) Créer la nouvelle WorkSession (données de démarrage uniquement)
            currentSession = new WorkSession
            {
                StartTime = DateTime.Now,
                StartLatitude = lat,
                StartLongitude = lon,
                StartAddress = startAddress,
                SessionType = SelectedSessionType,
                IncludesTravelTime = IncludesTravelTime,
                TravelDurationHours = null,               // défini au ClockOut
                DinnerPaid = DinnerPaidBy.None,
                Location = startAddress
            };

            // Réinitialiser les champs de saisie de trajet
            TravelHours = string.Empty;
            TravelMinutes = string.Empty;
        }

        private async void OnClockOut()
        {
            if (currentSession == null)
                return;

            // 1) Récupérer la position GPS de fin
            var location = await _locationService.GetCurrentLocationAsync();

            string endAddress = "Location unavailable";
            double lat = 0, lon = 0;

            if (location != null)
            {
                lat = location.Latitude;
                lon = location.Longitude;
                endAddress = await LocationHelper.GetAddressFromCoordinatesAsync(lat, lon);
            }

            // 2) Calculer TravelDurationHours à partir de TravelHours / TravelMinutes
            if (IncludesTravelTime
                && int.TryParse(TravelHours, out var h)
                && int.TryParse(TravelMinutes, out var m))
            {
                currentSession.TravelDurationHours = h + (m / 60.0);
            }
            else
            {
                currentSession.TravelDurationHours = null;
            }

            // 3) Compléter le reste et sauvegarder
            currentSession.EndTime = DateTime.Now;
            currentSession.EndLatitude = lat;
            currentSession.EndLongitude = lon;
            currentSession.EndAddress = endAddress;
            currentSession.DinnerPaid = SelectedDinnerPaidBy;

            await _sessionService.EndAndSaveCurrentSessionAsync();
            currentSession = null;
        }

        // ════ INotifyPropertyChanged ════
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private void SetProperty<T>(
            ref T backingStore,
            T value,
            [CallerMemberName] string propName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                backingStore = value;
                OnPropertyChanged(propName);
            }
        }
    }
}
