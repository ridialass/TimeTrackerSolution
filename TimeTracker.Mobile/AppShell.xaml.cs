using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Resources.Strings; // Ajout pour l'i18n

namespace TimeTracker.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("HomePage", typeof(Views.HomePage));
            Routing.RegisterRoute("AdminDashboardPage", typeof(Views.AdminDashboardPage));
            Routing.RegisterRoute("StartSessionPage", typeof(Views.StartSessionPage));
            Routing.RegisterRoute("EndSessionPage", typeof(Views.EndSessionPage));
            Routing.RegisterRoute("TimeEntriesPage", typeof(Views.TimeEntriesPage));

            // Toujours démarrer sur LoginPage
            SetLoginFlyoutAsCurrent();
        }

        /// <summary>
        /// Nettoie tous les FlyoutItems/MenuItems sauf LoginPage.
        /// </summary>
        private void ResetToLoginShell()
        {
            foreach (var item in Items.ToList())
            {
                if (item is FlyoutItem flyout && flyout.Route == "LoginPage") continue;
                Items.Remove(item);
            }
            foreach (var menuItem in Items.OfType<MenuItem>().ToList())
                Items.Remove(menuItem);
        }

        /// <summary>
        /// Met LoginPage comme item actif.
        /// </summary>
        private void SetLoginFlyoutAsCurrent()
        {
            var loginFlyoutItem = Items.OfType<FlyoutItem>().FirstOrDefault(i => i.Route == "LoginPage");
            if (loginFlyoutItem != null)
                CurrentItem = loginFlyoutItem;
        }

        /// <summary>
        /// Appelé APRÈS login pour afficher le menu selon le rôle.
        /// </summary>
        public void ConfigureFlyoutForRole(string role)
        {
            // Nettoie le menu sauf LoginPage
            foreach (var item in Items.ToList())
            {
                if (item is FlyoutItem flyout && flyout.Route == "LoginPage") continue;
                if (item is FlyoutItem || item is TabBar)
                    Items.Remove(item);
            }
            foreach (var menuItem in Items.OfType<MenuItem>().ToList())
                Items.Remove(menuItem);

            // Ajoute le menu selon le rôle
            if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, out var userRole))
            {
                switch (userRole)
                {
                    case UserRole.Admin:
                        Items.Add(new FlyoutItem
                        {
                            Title = AppResources.AdminDashboard_Title, // i18n
                            Route = "AdminDashboardPage",
                            Items =
                            {
                                new ShellContent
                                {
                                    Title = AppResources.AdminDashboard_Tab, // i18n
                                    ContentTemplate = new DataTemplate(() =>
                                    {
                                        var page = App.ServiceProvider?.GetService<Views.AdminDashboardPage>();
                                        if (page == null)
                                            throw new InvalidOperationException("AdminDashboardPage is not registered in DI.");
                                        return page;
                                    }),
                                    Route = "AdminDashboardPage"
                                }
                            }
                        });
                        break;
                    case UserRole.Employee:
                        goto default;
                    default:
                        Items.Add(new FlyoutItem
                        {
                            Title = AppResources.Home_Title, // i18n
                            Route = "HomePage",
                            Items =
                            {
                                new ShellContent
                                {
                                    Title = AppResources.Home_Tab, // i18n
                                    ContentTemplate = new DataTemplate(() =>
                                    {
                                        var page = App.ServiceProvider?.GetService<Views.HomePage>();
                                        if (page == null)
                                            throw new InvalidOperationException("HomePage is not registered in DI.");
                                        return page;
                                    }),
                                    Route = "HomePage"
                                }
                            }
                        });
                        break;
                }
            }

            // Ajoute le logout
            Items.Add(new MenuItem
            {
                Text = AppResources.Logout, // i18n
                Command = new Command(async () =>
                {
                    if (Application.Current is App app)
                        await app.LogoutAsync();
                })
            });
        }

        /// <summary>
        /// Appelé lors du logout pour remettre le Shell à l’état LoginPage.
        /// </summary>
        public async Task ResetForLogoutAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Début ResetForLogoutAsync ===");

                foreach (var item in Items)
                    System.Diagnostics.Debug.WriteLine("[Shell BEFORE] " + (item as FlyoutItem)?.Route);

                ResetToLoginShell();

                foreach (var item in Items)
                    System.Diagnostics.Debug.WriteLine("[Shell AFTER] " + (item as FlyoutItem)?.Route);

                SetLoginFlyoutAsCurrent();

                System.Diagnostics.Debug.WriteLine("[Shell CURRENT] " + (CurrentItem as FlyoutItem)?.Route);

                FlyoutBehavior = FlyoutBehavior.Disabled;

                // NE NAVIGUE VERS //LoginPage QUE SI TU N'Y ES PAS DÉJÀ
                if ((CurrentItem as FlyoutItem)?.Route != "LoginPage")
                {
                    System.Diagnostics.Debug.WriteLine("=== Juste avant GoToAsync(\"//LoginPage\") ===");
                    await GoToAsync("//LoginPage", true);
                    System.Diagnostics.Debug.WriteLine("=== Après GoToAsync(\"//LoginPage\") ===");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Déjà sur LoginPage, aucune navigation nécessaire.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ResetForLogoutAsync] Exception: {ex}");
                throw;
            }
        }
    }
}