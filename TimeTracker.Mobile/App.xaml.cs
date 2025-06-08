using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile
{
    public partial class App : Application
    {
        private readonly MobileAuthService _authService;

        public App(MobileAuthService authService)
        {
            InitializeComponent();
            _authService = authService;

            MainPage = new NavigationPage(new LoginPage());
            Task.Run(async () => await TryRestoreSessionOnLaunch());
        }

        private async Task TryRestoreSessionOnLaunch()
        {
            bool hasSession = await _authService.TryRestoreSessionAsync();
            if (hasSession)
            {
                var role = _authService.CurrentUser!.Role;
                if (role == Core.Enums.UserRole.Admin)
                    MainPage = new NavigationPage(new AdminDashboardPage());
                else
                    MainPage = new NavigationPage(new HomePage());
            }
        }
    }
}
