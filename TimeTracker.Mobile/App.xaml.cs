using Microsoft.Extensions.Logging;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile;

public partial class App : Application
{
    private readonly IAuthService _authService;
    private readonly IServiceProvider _services;
    private readonly ILogger<App> _logger;

    public App(IAuthService authService, IServiceProvider services, ILogger<App> logger, AppShell shell)
    {
        InitializeComponent();

        _authService = authService;
        _services = services;
        _logger = logger;

        MainPage = shell;

        // ⚠️ Lance l’authentification après affichage du Shell
        MainPage.Dispatcher.Dispatch(async () => await TryRestoreSessionOnLaunch());
    }

    private async Task TryRestoreSessionOnLaunch()
    {
        try
        {
            var shell = Shell.Current;

            // 1️⃣ Si aucune session : LoginPage est déjà affichée
            if (!await _authService.TryRestoreSessionAsync())
            {
                shell.FlyoutBehavior = FlyoutBehavior.Disabled;
                return;
            }

            // 2️⃣ Session restaurée → activer menu
            shell.FlyoutBehavior = FlyoutBehavior.Flyout;

            // 3️⃣ Configurer menu selon rôle
            if (shell is AppShell appShell)
                appShell.ConfigureFlyoutForRole(_authService.CurrentUser!.Role);

            // 4️⃣ Navigation directe vers HomePage ou Dashboard
            var role = _authService.CurrentUser?.Role ?? string.Empty;

            if (Enum.TryParse<UserRole>(role, out var userRole))
            {
                var target = userRole == UserRole.Admin ? "AdminDashboardPage" : "HomePage";

                // 🧼 Nettoyage de pile + navigation absolue
                shell.Items.Clear();
                await shell.GoToAsync($"//{target}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur au démarrage de l'application");
            await Shell.Current.DisplayAlert("Erreur", "Une erreur s’est produite au lancement.", "OK");
        }
    }
}
