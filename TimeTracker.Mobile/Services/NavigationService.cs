using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile.Services
{
    public class NavigationService : INavigationService
    {
        public Task GoToLoginPageAsync()
            => Shell.Current.GoToAsync("//LoginPage");

        public Task GoToHomePageAsync()
            => Shell.Current.GoToAsync("//HomePage");

        public Task GoToAdminDashboardPageAsync()
            => Shell.Current.GoToAsync("//AdminDashboardPage");

        public Task GoToStartSessionPageAsync()
            => Shell.Current.GoToAsync("StartSessionPage");

        public Task GoToEndSessionPageAsync()
            => Shell.Current.GoToAsync("EndSessionPage");

        public Task GoToTimeEntriesPageAsync()
            => Shell.Current.GoToAsync("TimeEntriesPage");

        public Task GoBackAsync()
            => Shell.Current.GoToAsync("..");
    }
}