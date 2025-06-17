using Microsoft.Extensions.Logging;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile
{
    public partial class App : Application
    {
        private readonly IMobileAuthService _authService;
        private readonly IServiceProvider _services;
        private readonly ILogger<App> _logger;

        public App(
            IMobileAuthService authService,
            IServiceProvider services,
            ILogger<App> logger)
        {
            InitializeComponent();

            _authService = authService;
            _services = services;
            _logger = logger;

            // 1) Souscription aux exceptions .NET non gérées
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            // 2) Page d’entrée
            MainPage = new NavigationPage(services.GetRequiredService<LoginPage>());

            // 3) Restauration de session en tâche de fond
            Task.Run(async () => await TryRestoreSessionOnLaunch());
        }

        private void OnDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            _logger.LogError(ex, "Exception non gérée en domaine d'application");

            // Vous pouvez afficher une alerte UI si vous le souhaitez :
            MainPage?.Dispatcher.Dispatch(async () =>
            {
                await MainPage.DisplayAlert(
                    "Erreur critique",
                    ex?.Message ?? "Une erreur inattendue est survenue.",
                    "OK");
            });
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger.LogError(e.Exception, "Exception de tâche non observée");

            // On lève un flag pour éviter que le process ne crashe
            e.SetObserved();
        }

        private async Task TryRestoreSessionOnLaunch()
        {
            if (!await _authService.TryRestoreSessionAsync())
                return;

            var role = _authService.CurrentUser!.Role;
            if (role == Core.Enums.UserRole.Admin)
            {
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
