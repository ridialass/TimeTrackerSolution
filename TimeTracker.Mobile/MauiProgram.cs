using Microsoft.Extensions.Logging;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.ViewModels;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ──────────────
        // 👉 Logging
        builder.Logging.AddDebug();

        var services = builder.Services;

        // ──────────────
        // 👉 Services de stockage et navigation
        services.AddSingleton<App>();
        services.AddSingleton<ISecureStorage>(SecureStorage.Default);
        services.AddSingleton<ISecureStorageService, SecureStorageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IGeolocationService, GeolocationService>();
        services.AddSingleton<ISessionStateService, SessionStateService>();

        // ──────────────
        // 👉 Handlers HTTP
        services.AddTransient<AuthHeaderHandler>();

        services.AddHttpClient<IApiClientService, ApiClientService>(client =>
        {
#if ANDROID
            client.BaseAddress = new Uri("http://192.168.187.110:7205/");
#else
            client.BaseAddress = new Uri("https://localhost:7205/");
#endif
        }).AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddHttpClient<IMobileTimeEntryService, MobileTimeEntryService>(client =>
        {
#if ANDROID
            client.BaseAddress = new Uri("http://192.168.187.110:7205/");
#else
            client.BaseAddress = new Uri("https://localhost:7205/");
#endif
        }).AddHttpMessageHandler<AuthHeaderHandler>();

        // ──────────────
        // 👉 Services métiers
        services.AddSingleton<IAuthService, AuthService>();

        // ──────────────
        // 👉 ViewModels (Transient)
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegistrationViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<StartSessionViewModel>();
        services.AddTransient<EndSessionViewModel>();
        services.AddTransient<AdminDashboardViewModel>();
        services.AddTransient<TimeEntriesViewModel>();

        // ──────────────
        // 👉 Views (navigation MAUI)
        services.AddTransient<LoginPage>();
        services.AddTransient<RegistrationPage>();
        services.AddTransient<HomePage>();
        services.AddTransient<StartSessionPage>();
        services.AddTransient<EndSessionPage>();
        services.AddTransient<AdminDashboardPage>();
        services.AddTransient<TimeEntriesPage>();

        // ──────────────
        // 👉 Shell & App
        services.AddSingleton<AppShell>();
        services.AddSingleton<App>();

        return builder.Build();
    }
}