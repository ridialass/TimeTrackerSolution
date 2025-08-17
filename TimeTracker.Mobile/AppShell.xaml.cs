
using Microsoft.Maui.ApplicationModel; // MainThread
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Resources.Strings;

namespace TimeTracker.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Routes
            Routing.RegisterRoute("HomePage", typeof(Views.HomePage));
            Routing.RegisterRoute("AdminDashboardPage", typeof(Views.AdminDashboardPage));
            Routing.RegisterRoute("StartSessionPage", typeof(Views.StartSessionPage));
            Routing.RegisterRoute("EndSessionPage", typeof(Views.EndSessionPage));
            Routing.RegisterRoute("TimeEntriesPage", typeof(Views.TimeEntriesPage));

            FlyoutBehavior = FlyoutBehavior.Disabled;
            SetLoginFlyoutAsCurrent();
        }

        private void SetLoginFlyoutAsCurrent()
        {
            var login = Items.OfType<FlyoutItem>().FirstOrDefault(i => i.Route == "LoginPage");
            if (login is not null) CurrentItem = login;
        }

        private void ResetToLoginShell()
        {
            // Remove all FlyoutItem/TabBar except LoginPage
            foreach (var item in Items.ToList())
            {
                if (item is FlyoutItem fi && fi.Route == "LoginPage") continue;
                Items.Remove(item);
            }

            // Remove ALL menu items (use MenuItems, not Items)
            foreach (var item in Items.ToList())
            {
                if (item is MenuItem)
                    Items.Remove(item);
            }
        }

        public async Task ConfigureFlyoutForRoleAsync(string role)
        {
            ResetToLoginShell();

            // Build role-specific flyout (defaults to Home if parse fails)
            if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, out var userRole))
            {
                switch (userRole)
                {
                    case UserRole.Admin:
                        Items.Add(new FlyoutItem
                        {
                            Title = AppResources.AdminDashboard_Title,
                            Route = "AdminDashboardPage",
                            Items =
                            {
                                new ShellContent
                                {
                                    Title = AppResources.AdminDashboard_Tab,
                                    Route = "AdminDashboardPage",
                                    ContentTemplate = new DataTemplate(() => App.GetService<Views.AdminDashboardPage>())
                                }
                            }
                        });
                        CurrentItem = Items.OfType<FlyoutItem>().First(i => i.Route == "AdminDashboardPage");
                        await GoToAsync("//AdminDashboardPage", true);
                        break;

                    default: // Employee & others => Home
                        goto case UserRole.Employee;

                    case UserRole.Employee:
                        Items.Add(new FlyoutItem
                        {
                            Title = AppResources.Home_Title,
                            Route = "HomePage",
                            Items =
                            {
                                new ShellContent
                                {
                                    Title = AppResources.Home_Tab,
                                    Route = "HomePage",
                                    ContentTemplate = new DataTemplate(() => App.GetService<Views.HomePage>())
                                }
                            }
                        });
                        CurrentItem = Items.OfType<FlyoutItem>().First(i => i.Route == "HomePage");
                        await GoToAsync("//HomePage", true);
                        break;
                }
            }
            else
            {
                // Default to Home
                Items.Add(new FlyoutItem
                {
                    Title = AppResources.Home_Title,
                    Route = "HomePage",
                    Items =
                    {
                        new ShellContent
                        {
                            Title = AppResources.Home_Tab,
                            Route = "HomePage",
                            ContentTemplate = new DataTemplate(() => App.GetService<Views.HomePage>())
                        }
                    }
                });
                CurrentItem = Items.OfType<FlyoutItem>().First(i => i.Route == "HomePage");
                await GoToAsync("//HomePage", true);
            }

            // Add Logout to MenuItems (NOT to Items)
            Items.Add(new MenuItem
            {
                Text = AppResources.Logout,
                Command = new Command(async () =>
                {
                    if (Application.Current is App app)
                        await app.LogoutAsync();
                })
            });

            FlyoutBehavior = FlyoutBehavior.Flyout;
        }

        // Back-compat for old calls without "Async"
        public void ConfigureFlyoutForRole(string role)
            => _ = MainThread.InvokeOnMainThreadAsync(() => ConfigureFlyoutForRoleAsync(role));

        public async Task ResetForLogoutAsync()
        {
            try
            {
                ResetToLoginShell();
                SetLoginFlyoutAsCurrent();
                FlyoutBehavior = FlyoutBehavior.Disabled;

                if ((CurrentItem as FlyoutItem)?.Route != "LoginPage")
                    await GoToAsync("//LoginPage", true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ResetForLogoutAsync] Exception: {ex}");
                throw;
            }
        }
    }
}
