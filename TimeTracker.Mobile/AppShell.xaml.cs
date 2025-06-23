using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    private void RegisterRoutes()
    {
        // Les routes utilisées dans navigation doivent être enregistrées ici
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
        Routing.RegisterRoute(nameof(AdminDashboardPage), typeof(AdminDashboardPage));
        // Ajoute ici d'autres routes si tu as des pages secondaires (ex: TimeEntriesPage, etc.)
    }

    /// <summary>
    /// Active dynamiquement les pages du menu latéral en fonction du rôle.
    /// </summary>
    public void ConfigureFlyoutForRole(string role)
    {
        Items.Clear();

        if (role == "Admin")
        {
            Items.Add(new FlyoutItem
            {
                Title = "Dashboard",
                Route = "AdminDashboardPage",
                Items =
                {
                    new ShellContent
                    {
                        Title = "Admin Dashboard",
                        ContentTemplate = new DataTemplate(typeof(AdminDashboardPage)),
                        Route = "AdminDashboardPage"
                    }
                }
            });
        }
        else
        {
            Items.Add(new FlyoutItem
            {
                Title = "Accueil",
                Route = "HomePage",
                Items =
                {
                    new ShellContent
                    {
                        Title = "Accueil",
                        ContentTemplate = new DataTemplate(typeof(HomePage)),
                        Route = "HomePage"
                    }
                }
            });
        }
    }
}
