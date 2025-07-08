using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TimeTracker.Mobile.Services;

namespace TimeTracker.Mobile;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = default!;

    private readonly ISessionStateService _session;
    private readonly ILogger<App> _logger;

    public App(
    ISessionStateService session,
    ILogger<App> logger,
    AppShell shell,
    IServiceProvider provider)
    {
        InitializeComponent();

        ServiceProvider = provider;

        _session = session;
        _logger = logger;
        MainPage = shell;

        // Laisse le temps au Shell d'être actif avant de naviguer
        MainPage.Dispatcher.Dispatch(async () => {
            await Task.Delay(100); // <= Ajoute ce délai
            await TryRestoreSessionOnLaunch();
        });
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
            // On vérifie que la page active est bien un Shell pour éviter une exception ici aussi
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("Erreur", "Une erreur s’est produite au lancement.", "OK");
        }
    }

    public async Task LogoutAsync()
        => await _session.LogoutAsync();
}