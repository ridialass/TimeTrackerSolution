using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TimeTracker.Mobile.Services;

namespace TimeTracker.Mobile;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = default!;

    private readonly ISessionStateService _session;
    private readonly ILogger<App> _logger;

    // Ajoute IServiceProvider provider aux paramètres du constructeur
    public App(
        ISessionStateService session,
        ILogger<App> logger,
        AppShell shell,
        IServiceProvider provider)
    {
        InitializeComponent();

        ServiceProvider = provider; // <- Stocke le provider dans la propriété statique

        _session = session;
        _logger = logger;
        MainPage = shell;

        MainPage.Dispatcher.Dispatch(async () => await TryRestoreSessionOnLaunch());
    }

    private async Task TryRestoreSessionOnLaunch()
    {
        try
        {
            await _session.TryRestoreSessionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur au démarrage de l'application");
            await Shell.Current.DisplayAlert("Erreur", "Une erreur s’est produite au lancement.", "OK");
        }
    }

    public async Task LogoutAsync() => await _session.LogoutAsync();
}