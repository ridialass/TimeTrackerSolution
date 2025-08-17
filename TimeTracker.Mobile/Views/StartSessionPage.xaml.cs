using TimeTracker.Mobile.Services.Interfaces;
using TimeTracker.Mobile.ViewModels;

namespace TimeTracker.Mobile.Views;

public partial class StartSessionPage : ContentPage
{
    private readonly INavigationService _nav;

    // DI: StartSessionViewModel comes from the container
    public StartSessionPage(StartSessionViewModel vm, INavigationService nav)
    {
        InitializeComponent();
        BindingContext = vm;
        _nav = nav;
    }

    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        try { await _nav.GoToHomePageAsync(); }
        catch (Exception ex)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[StartSessionPage] BackToHome error: {ex}");
#endif
        }
    }
}
