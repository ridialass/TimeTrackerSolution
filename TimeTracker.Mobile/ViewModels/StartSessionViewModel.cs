using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.Helpers;
using Microsoft.Maui.Devices.Sensors;
using static TimeTracker.Models.Enum;
using TimeTracker.Views;

namespace TimeTracker.ViewModels
{
    public class StartSessionViewModel : INotifyPropertyChanged
    {
        // ════ SOURCES DÉNUMÉRÉES ════
        public ObservableCollection<WorkSessionType> SessionTypes { get; }
            = new ObservableCollection<WorkSessionType>(
                (WorkSessionType[])System.Enum.GetValues(typeof(WorkSessionType)));

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

        // ════ COMMANDE ════
        public ICommand StartCommand { get; }

        // ════ ÉTAT ET SERVICES ════
        private readonly LocationService _locationService = new LocationService();
        private readonly SessionService _sessionService = App.SessionService;
        private readonly AuthService _authService = App.AuthService;

        public StartSessionViewModel()
        {
            StartCommand = new Command(OnStartSession);
        }

        private async void OnStartSession()
        {
            // 1) Récupérer la position GPS de début
            var location = await _locationService.GetCurrentLocationAsync();

            string address = "Location unavailable";
            double lat = 0, lon = 0;

            if (location != null)
            {
                lat = location.Latitude;
                lon = location.Longitude;
                address = await LocationHelper.GetAddressFromCoordinatesAsync(lat, lon);
            }

            // NOUVEAU : récupérer l’utilisateur actuellement connecté
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                // si, par erreur, aucun utilisateur n’est connecté, on peut afficher une alerte ou faire un bail out
                await Shell.Current.DisplayAlert(
                    "Error",
                    "No user is currently logged in.",
                    "OK");
                return;
            }

            // 2) Créer la WorkSession en cours
            var newSession = new WorkSession
            {
                StartTime = DateTime.Now,
                StartLatitude = lat,
                StartLongitude = lon,
                StartAddress = address,
                SessionType = SelectedSessionType,
                IncludesTravelTime = IncludesTravelTime,
                TravelDurationHours = null,              // sera défini plus tard
                DinnerPaid = DinnerPaidBy.None,
                Location = address,
                // ▲ AFFECTER l’ID et le nom utilisateur
                UserId = currentUser.Id,
                Username = currentUser.Username
            };

            // 3) Démarrer la session (méthode sérialise dans Preferences)
            _sessionService.StartNewSession(newSession);

            // 4) Naviguer vers EndSessionPage
            await Shell.Current.GoToAsync(nameof(EndSessionPage));
        }

        // ════ INotifyPropertyChanged ════
        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty<T>(
            ref T backingStore,
            T value,
            [CallerMemberName] string propName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                backingStore = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
