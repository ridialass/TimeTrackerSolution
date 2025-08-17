using System;
using System.Globalization;
using System.Net.Http.Headers;
using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using Polly;
using Polly.Extensions.Http;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Services.Interfaces;
using TimeTracker.Mobile.ViewModels;
using TimeTracker.Mobile.Views;
#if ANDROID
using Xamarin.Android.Net;
#endif

namespace TimeTracker.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // --- Localisation: enforce Italian for UI + current thread defaults
        var it = new CultureInfo("it-IT");
        CultureInfo.DefaultThreadCurrentCulture = it;
        CultureInfo.DefaultThreadCurrentUICulture = it;

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()

            // CommunityToolkit (Converters, Behaviors, etc.)
            .UseMauiCommunityToolkit()

            // Fonts
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var services = builder.Services;

        // ---- Platform services
        services.AddSingleton<ISecureStorage>(SecureStorage.Default);
        services.AddSingleton<IPreferences>(Preferences.Default);

        // ---- App services
        services.AddSingleton<ISecureStorageService, SecureStorageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IGeolocationService, GeolocationService>();
        services.AddSingleton<ISessionStateService, SessionStateService>();
        services.AddSingleton<ILocalStorageService, LocalStorageService>();

        // ---- HTTP handlers (DI-friendly)
        services.AddTransient<AuthHeaderHandler>();
        // If you later add a LoggingHandler to inspect JSON traffic, register it here:
        // services.AddTransient<LoggingHandler>();

        // ---- Shared Http policy (retry network hiccups + HTTP 5xx + 429)
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => (int)msg.StatusCode == 429)
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5)
            });

        // ---- Named HttpClient used elsewhere via IHttpClientFactory.CreateClient("Api")
        // SECURITY: Always HTTPS in prod. https://10.0.2.2 is for Android emulator talking to host.
        services.AddHttpClient("Api", client =>
        {
#if ANDROID
            client.BaseAddress = new Uri("https://10.0.2.2:7205/");
#else
            client.BaseAddress = new Uri("https://localhost:7205/");
#endif
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
#if ANDROID && DEBUG
        .ConfigurePrimaryHttpMessageHandler(() => new AndroidMessageHandler
        {
            // ⚠️ DEBUG ONLY: trust dev cert even if hostname mismatch (localhost vs 10.0.2.2)
            ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
        })
#else
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        })
#endif
        .AddHttpMessageHandler<AuthHeaderHandler>()
        // .AddHttpMessageHandler<LoggingHandler>() // enable when you add it
        .AddPolicyHandler(retryPolicy);

        // ---- TYPED client for IApiClientService (used by AuthService, etc.)
        services.AddHttpClient<IApiClientService, ApiClientService>(client =>
        {
#if ANDROID
            client.BaseAddress = new Uri("https://10.0.2.2:7205/");
#else
            client.BaseAddress = new Uri("https://localhost:7205/");
#endif
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
#if ANDROID && DEBUG
        .ConfigurePrimaryHttpMessageHandler(() => new AndroidMessageHandler
        {
            ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
        })
#else
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        })
#endif
        .AddHttpMessageHandler<AuthHeaderHandler>()
        // .AddHttpMessageHandler<LoggingHandler>() // enable when you add it
        .AddPolicyHandler(retryPolicy);

        // ---- Domain services (keep your local-first design)
        services.AddSingleton<IMobileTimeEntryService, MobileTimeEntryService>();
        services.AddSingleton<IAuthService, AuthService>();

        // ---- ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegistrationViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<StartSessionViewModel>();
        services.AddTransient<EndSessionViewModel>();
        services.AddTransient<AdminDashboardViewModel>();
        services.AddTransient<TimeEntriesViewModel>();

        // ---- Views
        services.AddTransient<LoginPage>();
        services.AddTransient<RegistrationPage>();
        services.AddTransient<HomePage>();
        services.AddTransient<StartSessionPage>();
        services.AddTransient<EndSessionPage>();
        services.AddTransient<AdminDashboardPage>();
        services.AddTransient<TimeEntriesPage>();

        // ---- Shell & App
        services.AddSingleton<AppShell>();
        services.AddSingleton<App>();

        return builder.Build();
    }
}