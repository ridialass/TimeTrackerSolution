using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Resources.Strings; // i18n
using TimeTracker.Mobile.Services;
using Microsoft.Maui.Controls;

namespace TimeTracker.Mobile.ViewModels
{
    public partial class TimeEntriesViewModel : BaseViewModel
    {
        private readonly IApiClientService _apiClient;
        private readonly IAuthService _authService;
        private readonly IDialogService _dialogs;

        public TimeEntriesViewModel(
            IApiClientService apiClient,
            IAuthService authService,
            IDialogService dialogs)
        {
            _apiClient = apiClient;
            _authService = authService;
            _dialogs = dialogs;
        }

        // ------- Liste des pointages -------
        private ObservableCollection<TimeEntryDto> timeEntries = new();
        public ObservableCollection<TimeEntryDto> TimeEntries
        {
            get => timeEntries;
            set => SetProperty(ref timeEntries, value);
        }

        // État "liste vide"
        private bool isEmpty;
        public bool IsEmpty
        {
            get => isEmpty;
            set => SetProperty(ref isEmpty, value);
        }

        // Pull-to-refresh (déclaration explicite pour éviter les soucis de generator)
        private bool isRefreshing;
        public bool IsRefreshing
        {
            get => isRefreshing;
            set => SetProperty(ref isRefreshing, value);
        }

        // -------- Commands --------

        [RelayCommand]
        public async Task LoadTimeEntriesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var user = _authService.CurrentUser;
                if (user == null)
                {
                    ErrorMessage = AppResources.TimeEntries_NotAuthenticated;
                    TimeEntries.Clear();
                    IsEmpty = true;
                    return;
                }

                var result = await _apiClient.GetTimeEntriesAsync(user.Id);
                if (result.IsSuccess && result.Value is not null)
                {
                    timeEntries.Clear();
                    foreach (var entry in result.Value) // affiche toutes les entrées (terminées ou non)
                        timeEntries.Add(entry);

                    IsEmpty = timeEntries.Count == 0;
                }
                else
                {
                    ErrorMessage = result.Error ?? AppResources.TimeEntries_FailedToLoad;
                    TimeEntries.Clear();
                    IsEmpty = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TimeEntries] Load error: {ex.Message}");
                ErrorMessage = AppResources.TimeEntries_FailedToLoad;
                TimeEntries.Clear();
                IsEmpty = true;
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false; // important pour RefreshView
            }
        }

        [RelayCommand]
        public Task RefreshAsync() => LoadTimeEntriesAsync();

        /// <summary>
        /// Ouvre l’écran d’édition pour l’entrée sélectionnée, après avoir validé que l’API la retourne bien.
        /// </summary>
        [RelayCommand]
        public async Task EditTimeEntryAsync(TimeEntryDto? entry)
        {
            if (entry is null || entry.Id <= 0) return;

            try
            {
                var res = await _apiClient.GetTimeEntryByIdAsync(entry.Id);
                if (!res.IsSuccess || res.Value is null)
                {
                    await _dialogs.ShowErrorAsync(AppResources.TimeEntries_FailedToLoad);
                    return;
                }

                // Navigation vers la page d’édition (passe l’Id, la page/VM rechargera si besoin)
                await Shell.Current.GoToAsync("EditTimeEntryPage", new Dictionary<string, object>
                {
                    ["TimeEntryId"] = entry.Id
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TimeEntries] Edit error: {ex.Message}");
                await _dialogs.ShowErrorAsync(AppResources.TimeEntries_FailedToLoad);
            }
        }
    }
}
