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

        // ───────────────────────────────
        // 👉 Logging
        builder.Logging.AddDebug();

        // ───────────────────────────────
        // 👉 DI Registrations
        var services = builder.Services;

        // Core Services
        builder.Services.AddSingleton<ISecureStorage>(SecureStorage.Default);
        services.AddSingleton<ISecureStorageService, SecureStorageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IGeolocationService, GeolocationService>();

        // Auth / API / Mobile services
        builder.Services.AddHttpClient<IApiClientService, ApiClientService>(client =>
        {
        #if ANDROID
                    client.BaseAddress = new Uri("https://10.0.2.2:7205/"); // ✅ Use host machine from Android emulator
        #else
            client.BaseAddress = new Uri("https://localhost:7205/");
        #endif
        })
        .AddHttpMessageHandler<AuthHeaderHandler>(); // JWT injector

        services.AddSingleton<AuthHeaderHandler>(); // injectable handler
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IMobileTimeEntryService, MobileTimeEntryService>();

        // ───────────────────────────────
        // 👉 ViewModels (Transient – new each time)
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegistrationViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<StartSessionViewModel>();
        services.AddTransient<EndSessionViewModel>();
        services.AddTransient<AdminDashboardViewModel>();
        services.AddTransient<TimeEntriesViewModel>();

        // ───────────────────────────────
        // 👉 Views (Navigation targets)
        services.AddTransient<LoginPage>();
        services.AddTransient<RegistrationPage>();
        services.AddTransient<HomePage>();
        services.AddTransient<StartSessionPage>();
        services.AddTransient<EndSessionPage>();
        services.AddTransient<AdminDashboardPage>();
        services.AddTransient<TimeEntriesPage>();

        // ───────────────────────────────
        // 👉 Shell & App
        services.AddSingleton<AppShell>();
        services.AddSingleton<App>();

        return builder.Build();
    }
}
