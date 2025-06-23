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

        // Lance la tentative de restauration de session après affichage du Shell
        MainPage.Dispatcher.Dispatch(async () => await TryRestoreSessionOnLaunch());
    }

    private async Task TryRestoreSessionOnLaunch()
    {
        try
        {
            var shell = Shell.Current;

            // Si aucune session : LoginPage reste affichée (seul ShellContent dans le Shell)
            if (!await _authService.TryRestoreSessionAsync())
            {
                shell.FlyoutBehavior = FlyoutBehavior.Disabled;
                return;
            }

            // Session restaurée → activer menu
            shell.FlyoutBehavior = FlyoutBehavior.Flyout;

            // Ajouter dynamiquement le menu selon le rôle
            if (shell is AppShell appShell)
                appShell.ConfigureFlyoutForRole(_authService.CurrentUser!.Role);

            var role = _authService.CurrentUser?.Role ?? string.Empty;

            if (Enum.TryParse<UserRole>(role, out var userRole))
            {
                var target = userRole == UserRole.Admin ? "AdminDashboardPage" : "HomePage";

                // Navigation absolue vers la page cible
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