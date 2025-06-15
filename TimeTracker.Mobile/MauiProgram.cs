using System;
using System.Net.Http.Headers;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.DependencyInjection;           // ← ici
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.ViewModels;
using TimeTracker.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace TimeTracker.Mobile
{
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

            // Ajout de la configuration du logging
            builder.Logging.AddDebug(); // Remplacement de ConfigureLogging par Logging.AddDebug()

            // 1) Handler pour ajouter le JWT
            builder.Services.AddTransient<AuthHeaderHandler>();

            // 2) Typed HttpClient pour votre API
            builder.Services
              .AddHttpClient<IApiClientService, ApiClientService>(client =>
              {
                  client.BaseAddress = new Uri("https://localhost:7205/");
                  client.DefaultRequestHeaders.Accept.Add(
                      new MediaTypeWithQualityHeaderValue("application/json"));
              })
              .AddHttpMessageHandler<AuthHeaderHandler>();

            // 3) SecureStorage singleton
            builder.Services.AddSingleton<ISecureStorage>(SecureStorage.Default);

            // 4) AuthService mobile
            builder.Services.AddSingleton<IMobileAuthService, MobileAuthService>();

            // 5) TimeEntryService mobile
            builder.Services.AddTransient<IMobileTimeEntryService, MobileTimeEntryService>();

            // 6) ViewModels
            builder.Services.AddTransient<MobileTimeEntryViewModel>();
            builder.Services.AddSingleton<IMobileTimeEntryService, MobileTimeEntryService>();
            builder.Services.AddSingleton<LocationService>();
            builder.Services.AddTransient<EndSessionViewModel>();
            builder.Services.AddSingleton<LocationService>();
            builder.Services.AddSingleton<IMobileTimeEntryService, MobileTimeEntryService>();
            builder.Services.AddTransient<StartSessionViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<AdminDashboardViewModel>();
            //… etc.

            // 7) Pages avec leur BindingContext
            builder.Services.AddTransient<TimeEntryPage>(sp =>
                new TimeEntryPage
                {
                    BindingContext = sp.GetRequiredService<MobileTimeEntryViewModel>()
                });
            builder.Services.AddTransient<EndSessionPage>(sp =>
                new EndSessionPage { BindingContext = sp.GetRequiredService<EndSessionViewModel>() });
            builder.Services.AddTransient<StartSessionPage>(sp =>
                new StartSessionPage { BindingContext = sp.GetRequiredService<StartSessionViewModel>() });
            builder.Services.AddTransient<LoginPage>(sp =>
                new LoginPage { BindingContext = sp.GetRequiredService<LoginViewModel>() });
            builder.Services.AddTransient<HomePage>(sp =>
                new HomePage { BindingContext = sp.GetRequiredService<HomeViewModel>() });
            builder.Services.AddTransient<AdminDashboardPage>(sp =>
                new AdminDashboardPage(sp.GetRequiredService<AdminDashboardViewModel>()));
            //… etc.

            return builder.Build();
        }
    }
}
