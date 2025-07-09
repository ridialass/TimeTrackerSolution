// SECURITE :
// Pour toute configuration HttpClient, toujours utiliser HTTPS en production.
// NE JAMAIS utiliser une baseAddress HTTP (même sur Android) pour un environnement de production ou de test réel.
// Vérifiez régulièrement que la configuration de prod pointe bien vers une URL HTTPS sécurisée (avec certificat valide).
// Ne jamais logger ni persister le mot de passe utilisateur ou des informations d’authentification dans la configuration ou lors des requêtes.

using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using System.Globalization;
using System.Threading;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.ViewModels;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("it"); // ou "it" ou selon la config de l'utilisateur
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit() // ← À la suite, ici !
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Logging.AddDebug();

        var services = builder.Services;

        // Services de stockage et navigation
        services.AddSingleton<App>();
        services.AddSingleton<ISecureStorage>(SecureStorage.Default);
        services.AddSingleton<ISecureStorageService, SecureStorageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IGeolocationService, GeolocationService>();
        services.AddSingleton<ISessionStateService, SessionStateService>();

        // Handlers HTTP
        services.AddTransient<AuthHeaderHandler>();

        // NOTE DE SECURITE :
        // En PROD, la baseAddress DOIT être en HTTPS avec certificat valide.
        // Pour du dev local sur Android, http://10.0.2.2 est toléré, mais NE JAMAIS déployer cette configuration.
        services.AddHttpClient<IApiClientService, ApiClientService>(client =>
        {
#if ANDROID
            client.BaseAddress = new Uri(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
                    ? "https://votre-api-production.com/"    // Remplacer par le vrai endpoint PROD
                    : "http://10.0.2.2:7205/"                // Dev local Android seulement !
            );
#else
            client.BaseAddress = new Uri(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
                    ? "https://votre-api-production.com/"    // Remplacer par le vrai endpoint PROD
                    : "https://localhost:7205/"
            );
#endif
        }).AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddHttpClient<IMobileTimeEntryService, MobileTimeEntryService>(client =>
        {
#if ANDROID
            client.BaseAddress = new Uri(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
                    ? "https://votre-api-production.com/"
                    : "http://10.0.2.2:7205/"
            );
#else
            client.BaseAddress = new Uri(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
                    ? "https://votre-api-production.com/"
                    : "https://localhost:7205/"
            );
#endif
        }).AddHttpMessageHandler<AuthHeaderHandler>();

        // Services métiers
        services.AddSingleton<IAuthService, AuthService>();

        // ViewModels (Transient)
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegistrationViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<StartSessionViewModel>();
        services.AddTransient<EndSessionViewModel>();
        services.AddTransient<AdminDashboardViewModel>();
        services.AddTransient<TimeEntriesViewModel>();

        // Views (navigation MAUI)
        services.AddTransient<LoginPage>();
        services.AddTransient<RegistrationPage>();
        services.AddTransient<HomePage>();
        services.AddTransient<StartSessionPage>();
        services.AddTransient<EndSessionPage>();
        services.AddTransient<AdminDashboardPage>();
        services.AddTransient<TimeEntriesPage>();

        // Shell & App
        services.AddSingleton<AppShell>();
        services.AddSingleton<App>();

        return builder.Build();
    }
}