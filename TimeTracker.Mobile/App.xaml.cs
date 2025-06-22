using Microsoft.Extensions.Logging;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile
{
    public partial class App : Application
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _services;
        private readonly ILogger<App> _logger;

        public App(
            IAuthService authService,
            IServiceProvider services,
            ILogger<App> logger,
            AppShell shell)
        {
            InitializeComponent();

            _authService = authService;
            _services = services;
            _logger = logger;

            // 1) Souscription aux exceptions .NET non gérées
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            // 2) Page d’entrée
            MainPage = shell;

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
            {
                await MainPage.Dispatcher.DispatchAsync(() =>
                    Shell.Current.GoToAsync(nameof(LoginPage))); // ✅ relative route
                return;
            }

            var roleString = _authService.CurrentUser!.Role;

            if (Enum.TryParse<Core.Enums.UserRole>(roleString, out var userRole) && userRole == Core.Enums.UserRole.Admin)
            {
                await MainPage.Dispatcher.DispatchAsync(() =>
                    Shell.Current.GoToAsync(nameof(AdminDashboardPage))); // ✅ relative
            }
            else
            {
                await MainPage.Dispatcher.DispatchAsync(() =>
                    Shell.Current.GoToAsync(nameof(HomePage))); // ✅ relative
            }
        }


        public static async Task InitializeAsync(IServiceProvider services)
        {
            var auth = services.GetRequiredService<IAuthService>();
            if (await auth.TryRestoreSessionAsync())
            {
                var shell = services.GetRequiredService<AppShell>();
                Application.Current.MainPage = shell;
                // optionally navigate inside
            }
        }
    }
}
