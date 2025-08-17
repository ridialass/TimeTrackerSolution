// TimeTracker.Mobile/ViewModels/AdminDashboardViewModel.cs
#nullable enable
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Resources.Strings;
using TimeTracker.Mobile.Services.Interfaces;

namespace TimeTracker.Mobile.ViewModels
{
    public partial class AdminDashboardViewModel : BaseViewModel
    {
        private readonly IApiClientService _apiClient;
        private readonly IAuthService _authService;
        private readonly INavigationService _navigation;

        public AdminDashboardViewModel(
            IApiClientService apiClient,
            IAuthService authService,
            INavigationService navigation)
        {
            _apiClient = apiClient;
            _authService = authService;
            _navigation = navigation;
        }

        // Liste des employés
        private ObservableCollection<EmployeeDto> users = new();
        public ObservableCollection<EmployeeDto> Users
        {
            get => users;
            set => SetProperty(ref users, value);
        }

        // État "liste vide"
        private bool isEmpty;
        public bool IsEmpty
        {
            get => isEmpty;
            set => SetProperty(ref isEmpty, value);
        }

        // Pull-to-refresh
        private bool isRefreshing;
        public bool IsRefreshing
        {
            get => isRefreshing;
            set => SetProperty(ref isRefreshing, value);
        }

        [RelayCommand]
        public async Task LoadUsersAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // Auth requise
                var current = _authService.CurrentUser;
                if (current is null)
                {
                    ErrorMessage = AppResources.TimeEntries_NotAuthenticated;
                    Users.Clear();
                    IsEmpty = true;
                    return;
                }

                // Vérifier le rôle Admin
                if (!Enum.TryParse<UserRole>(current.Role, true, out var role) || role != UserRole.Admin)
                {
                    ErrorMessage = AppResources.Home_AdminDashboard_Alert_AccessDenied;
                    Users.Clear();
                    IsEmpty = true;
                    return;
                }

                var result = await _apiClient.GetEmployeesAsync();
                if (result.IsSuccess && result.Value is not null)
                {
                    Users.Clear();
                    foreach (var u in result.Value)
                        Users.Add(u);

                    IsEmpty = Users.Count == 0;
                }
                else
                {
                    ErrorMessage = result.Error ?? AppResources.AdminDashboard_LoadUsers_Error;
                    Users.Clear();
                    IsEmpty = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Load error: {ex.Message}");
                ErrorMessage = AppResources.AdminDashboard_LoadUsers_Error;
                Users.Clear();
                IsEmpty = true;
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public Task RefreshAsync() => LoadUsersAsync();
    }
}
