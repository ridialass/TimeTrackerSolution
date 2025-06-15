using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Helpers;

namespace TimeTracker.Mobile.ViewModels
{
    public class StartSessionViewModel : INotifyPropertyChanged
    {
        // ── SERVICES INJECTÉS ────────────────────────────────────────
        private readonly IMobileAuthService _authService;
        private readonly IMobileTimeEntryService _timeEntryService;
        private readonly LocationService _locationService;

        // ── COMMANDES PUBLIQUES ─────────────────────────────────────
        public ICommand StartCommand { get; }

        // ════ PROPRIÉTÉS POUR LE XAML ════

        public ObservableCollection<WorkSessionType> SessionTypes { get; }
            = new ObservableCollection<WorkSessionType>(Enum.GetValues<WorkSessionType>());

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

        // ════ CONSTRUCTEUR ════
        public StartSessionViewModel(
            IMobileAuthService authService,
            IMobileTimeEntryService timeEntryService,
            LocationService locationService)
        {
            _authService = authService;
            _timeEntryService = timeEntryService;
            _locationService = locationService;

            StartCommand = new Command(async () => await OnStartSessionAsync());
        }

        // ════ MÉTHODE DE DÉBUT DE SESSION ════
        private async Task OnStartSessionAsync()
        {
            // 1) Récupérer position GPS
            var loc = await _locationService.GetCurrentLocationAsync();
            string address = "Localisation indisponible";
            double lat = 0, lon = 0;
            if (loc != null)
            {
                lat = loc.Latitude;
                lon = loc.Longitude;
                address = await LocationHelper.GetAddressFromCoordinatesAsync(lat, lon);
            }

            // 2) Vérifier utilisateur connecté
            var user = _authService.CurrentUser;
            if (user == null)
            {
                await Shell.Current.DisplayAlert(
                    "Erreur",
                    "Aucun utilisateur connecté.",
                    "OK");
                return;
            }

            // 3) Construire le DTO et l’envoyer à l’API
            var dto = new TimeEntryDto
            {
                UserId = user.Id,
                Username = user.UserName!,
                SessionType = SelectedSessionType,
                StartTime = DateTime.Now,
                IncludesTravelTime = IncludesTravelTime,
                // TravelDurationHours sera calculé au moment de la fin de session
                StartLatitude = lat,
                StartLongitude = lon,
                StartAddress = address,
                DinnerPaid = DinnerPaidBy.None,
                Location = address
            };

            
            try
            {
                await _timeEntryService.CreateTimeEntryAsync(dto);
            }
            catch
            {
                await Shell.Current.DisplayAlert(
                    "Erreur",
                    "Impossible de démarrer la session.",
                    "OK");
                return;
            }

            await Shell.Current.GoToAsync(nameof(TimeTracker.Mobile.Views.EndSessionPage));
            

            // 4) Naviguer vers la page de fin de session
            await Shell.Current.GoToAsync(nameof(TimeTracker.Mobile.Views.EndSessionPage));
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName] string? name = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(name);
            }
        }
        #endregion
    }
}
