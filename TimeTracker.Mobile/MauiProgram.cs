using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.ViewModels;
using TimeTracker.ViewModels;
using TimeTracker.Views;

namespace TimeTracker
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
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<AdminDashboardViewModel>();
            builder.Services.AddTransient<TimeEntriesViewModel>(sp =>
            {
                var auth = sp.GetRequiredService<IMobileAuthService>();
                if (auth.CurrentUser is null)
                    throw new InvalidOperationException("Utilisateur non connecté");
                return new TimeEntriesViewModel(
                    sp.GetRequiredService<IMobileTimeEntryService>(),
                    auth.CurrentUser.Id
                );
            });

            // 7) Pages avec leur BindingContext
            builder.Services.AddTransient<LoginPage>(sp =>
                new LoginPage { BindingContext = sp.GetRequiredService<LoginViewModel>() });
            builder.Services.AddTransient<HomePage>(sp =>
                new HomePage { BindingContext = sp.GetRequiredService<HomeViewModel>() });
            builder.Services.AddTransient<AdminDashboardPage>(sp =>
                new AdminDashboardPage { BindingContext = sp.GetRequiredService<AdminDashboardViewModel>() });
            builder.Services.AddTransient<TimeEntriesPage>(sp =>
                new TimeEntriesPage { BindingContext = sp.GetRequiredService<TimeEntriesViewModel>() });

            return builder.Build();
        }
    }
}
