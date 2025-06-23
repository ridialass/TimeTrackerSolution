using TimeTracker.Core.Enums;

namespace TimeTracker.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Déclare toutes les routes Shell nécessaires
            Routing.RegisterRoute("LoginPage", typeof(Views.LoginPage));
            Routing.RegisterRoute("HomePage", typeof(Views.HomePage));
            Routing.RegisterRoute("AdminDashboardPage", typeof(Views.AdminDashboardPage));
            Routing.RegisterRoute("StartSessionPage", typeof(Views.StartSessionPage));
            Routing.RegisterRoute("EndSessionPage", typeof(Views.EndSessionPage));
            Routing.RegisterRoute("TimeEntriesPage", typeof(Views.TimeEntriesPage));
            // Ajoute ici toute autre page qui utilise GoToAsync
            // Exemples :
            // Routing.RegisterRoute("ProfilePage", typeof(Views.ProfilePage));
            // Routing.RegisterRoute("SettingsPage", typeof(Views.SettingsPage));

        }

        /// <summary>
        /// Rajoute dynamiquement les items du menu selon le rôle
        /// </summary>
        public void ConfigureFlyoutForRole(string role)
        {
            Items.Clear();

            if (Enum.TryParse<UserRole>(role, out var userRole))
            {
                switch (userRole)
                {
                    case UserRole.Admin:
                        Items.Add(new FlyoutItem
                        {
                            Title = "Dashboard Admin",
                            Route = "AdminDashboardPage",
                            Items =
                            {
                                new ShellContent
                                {
                                    Title = "Dashboard",
                                    ContentTemplate = new DataTemplate(typeof(Views.AdminDashboardPage)),
                                    Route = "AdminDashboardPage"
                                }
                            }
                        });
                        break;
                    case UserRole.Employee:
                    default:
                        Items.Add(new FlyoutItem
                        {
                            Title = "Accueil",
                            Route = "HomePage",
                            Items =
                            {
                                new ShellContent
                                {
                                    Title = "Accueil",
                                    ContentTemplate = new DataTemplate(typeof(Views.HomePage)),
                                    Route = "HomePage"
                                }
                            }
                        });
                        break;
                }
            }
        }
    }
}