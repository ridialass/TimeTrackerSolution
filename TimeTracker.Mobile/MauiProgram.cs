using Microsoft.Extensions.Logging;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.ViewModels;
using TimeTracker.Mobile.Views;

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

            //builder.Services.AddRefitClient<IApiClient>()
            //    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://<your-api-url>/api/"));

            //builder.Services.AddSingleton<IApiClientWrapper, ApiClientWrapper>();

            //builder.Services.AddSingleton<LoginViewModel>();
            //builder.Services.AddSingleton<MainViewModel>();
            //builder.Services.AddTransient<HistoryViewModel>();

            //builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<MainPage>();
            //builder.Services.AddTransient<HistoryPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            // 1. Register Services
            builder.Services.AddSingleton<IApiClientService, ApiClientService>();
            builder.Services.AddSingleton<MobileAuthService>();
            builder.Services.AddSingleton<MobileTimeEntryService>();
            builder.Services.AddSingleton<LocationService>();

            // 2. Register ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<HomeViewModel>();
            // … register other viewmodels (TimeEntryViewModel, HistoryViewModel, etc.)

            // 3. Register Pages and their bindings
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<HomePage>();
            // … register other pages

            return builder.Build();
        }
    }
}
