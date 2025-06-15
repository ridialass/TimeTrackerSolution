using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Services;

namespace TimeTracker.Mobile.ViewModels
{
    public class MobileTimeEntryViewModel : INotifyPropertyChanged
    {
        private readonly IMobileAuthService _authService;
        private readonly IMobileTimeEntryService _timeEntryService;

        public ObservableCollection<TimeEntryDto> timeEntryRepo { get; }
            = new ObservableCollection<TimeEntryDto>();

        /// <summary>
        /// Commande pour rafraîchir la liste des sessions.
        /// </summary>
        public ICommand RefreshCommand { get; }

        public MobileTimeEntryViewModel(
            IMobileAuthService authService,
            IMobileTimeEntryService timeEntryService)
        {
            _authService = authService;
            _timeEntryService = timeEntryService;

            // on initialise la commande
            RefreshCommand = new Command(async () => await LoadSessionsAsync());

            // on lance le chargement initial
            _ = LoadSessionsAsync();
        }

        private async Task LoadSessionsAsync()
        {
            // on attend que l’utilisateur soit restauré
            var user = _authService.CurrentUser;
            if (user == null)
                return; // ou afficher un message d’erreur

            try
            {
                var sessions = await _timeEntryService.GetTimeEntriesAsync(user.Id);

                timeEntryRepo.Clear();
                foreach (var s in sessions)
                    timeEntryRepo.Add(s);
            }
            catch
            {
                // TODO : gérer l’erreur (log, message UI…)
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}
