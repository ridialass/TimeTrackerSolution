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

            return builder.Build();
        }
    }
}
