using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;

namespace TimeTracker.Mobile
{
    public partial class AppShell : Shell
    {
        private MenuItem? _logoutMenuItem;

        public AppShell()
        {
            InitializeComponent();

            // NE PAS ENREGISTRER LoginPage ici (déjà dans le XAML)
            Routing.RegisterRoute("HomePage", typeof(Views.HomePage));
            Routing.RegisterRoute("AdminDashboardPage", typeof(Views.AdminDashboardPage));
            Routing.RegisterRoute("StartSessionPage", typeof(Views.StartSessionPage));
            Routing.RegisterRoute("EndSessionPage", typeof(Views.EndSessionPage));
            Routing.RegisterRoute("TimeEntriesPage", typeof(Views.TimeEntriesPage));
        }

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
                                    ContentTemplate = new DataTemplate(() => App.ServiceProvider.GetRequiredService<Views.AdminDashboardPage>()),
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
                                    ContentTemplate = new DataTemplate(() => App.ServiceProvider.GetRequiredService<Views.HomePage>()),
                                    Route = "HomePage"
                                }
                            }
                        });
                        break;
                }
            }

            // Ajout du bouton Déconnexion APRÈS les FlyoutItem
            if (_logoutMenuItem == null)
            {
                _logoutMenuItem = new MenuItem
                {
                    Text = "Déconnexion",
                    Command = new Command(async () =>
                    {
                        if (Application.Current is App app)
                            await app.LogoutAsync();
                    })
                };
            }
            Items.Add(_logoutMenuItem);
        }

        public async Task ResetForLogoutAsync()
        {
            Items.Clear();
            CurrentItem = null;
            FlyoutBehavior = FlyoutBehavior.Disabled;

            // Injection DI pour LoginPage
            var loginShellContent = new ShellContent
            {
                Title = "Connexion",
                ContentTemplate = new DataTemplate(() => App.ServiceProvider.GetRequiredService<Views.LoginPage>()),
                Route = "LoginPage"
            };
            Items.Add(loginShellContent);

            // Sélectionne la LoginPage comme page active
            CurrentItem = loginShellContent;

            // Pas besoin de GoToAsync ici
            await Task.CompletedTask;
        }
    }
}