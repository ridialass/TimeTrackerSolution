using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile
{
    public partial class App : Application
    {
        private readonly MobileAuthService _authService;
        private readonly IServiceProvider _services;

        public App(MobileAuthService authService, IServiceProvider services)
        {
            InitializeComponent();
            _authService = authService;
            _services = services;

            MainPage = new NavigationPage(new LoginPage());
            Task.Run(async () => await TryRestoreSessionOnLaunch());
        }

        private async Task TryRestoreSessionOnLaunch()
        {
            if (!await _authService.TryRestoreSessionAsync())
                return;
            var role = _authService.CurrentUser!.Role;
                if (role == Core.Enums.UserRole.Admin)
            {
                // On récupère la page complète (VM + Page) depuis le container
                var adminPage = _services.GetRequiredService<AdminDashboardPage>();
                MainPage = new NavigationPage(adminPage);
            }
            else
            {
                var homePage = _services.GetRequiredService<HomePage>();
                MainPage = new NavigationPage(homePage);
            }

        }
    }
}
