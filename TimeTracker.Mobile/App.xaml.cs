using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TimeTracker.Mobile.Services.Interfaces;

namespace TimeTracker.Mobile;

public partial class App : Application
{
    // Resolve services via the MAUI handler's ServiceProvider.
    public static T GetService<T>() where T : notnull
    {
        if (Current?.Handler?.MauiContext?.Services is IServiceProvider services)
            return services.GetRequiredService<T>();

        throw new InvalidOperationException(
            $"Unable to resolve service for type '{typeof(T)}'. " +
            "Ensure the service is registered and App initialization has completed.");
    }

    private readonly ISessionStateService _session;
    private readonly ILogger<App> _logger;

    public App(
        ISessionStateService session,
        ILogger<App> logger,
        AppShell shell)
    {
        InitializeComponent();

        _session = session;
        _logger = logger;
        MainPage = shell;

#if DEBUG
        // Optional: catch unexpected exceptions early during development
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            _logger.LogError(e.ExceptionObject as Exception, "UnhandledException (AppDomain)");
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            _logger.LogError(e.Exception, "UnobservedTaskException");
            e.SetObserved();
        };
#endif

        // Restore AFTER Shell/Handler are ready to avoid race conditions
        MainPage.Dispatcher.Dispatch(async () =>
        {
            try
            {
                await Task.Yield();               // ensure Shell.Current is ready
                await TryRestoreSessionOnLaunch(); // token -> role menu -> nav
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during startup restore.");
            }
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
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("Erreur", "Une erreur s’est produite au lancement.", "OK");
        }
    }

    public Task LogoutAsync() => _session.LogoutAsync();
}