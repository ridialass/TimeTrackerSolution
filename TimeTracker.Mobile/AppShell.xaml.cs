using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Déclarer les routes DI-only (si pages non présentes dans le XAML above)
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
        Routing.RegisterRoute(nameof(AdminDashboardPage), typeof(AdminDashboardPage));
        Routing.RegisterRoute(nameof(StartSessionPage), typeof(StartSessionPage));
        Routing.RegisterRoute(nameof(EndSessionPage), typeof(EndSessionPage));
        Routing.RegisterRoute(nameof(TimeEntriesPage), typeof(TimeEntriesPage)); 
    }
}
