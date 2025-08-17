using TimeTracker.Mobile.Services.Interfaces;
using TimeTracker.Mobile.ViewModels;

namespace TimeTracker.Mobile.Views;

public partial class EndSessionPage : ContentPage
{
    private readonly EndSessionViewModel _vm;
    private readonly INavigationService _nav;

    public EndSessionPage(EndSessionViewModel vm, INavigationService nav)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _nav = nav;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await _vm.ReloadSessionAsync(); }
        catch (Exception ex)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[EndSessionPage] ReloadSession error: {ex}");
#endif
        }
    }

    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        try { await _nav.GoToHomePageAsync(); }
        catch (Exception ex)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[EndSessionPage] BackToHome error: {ex}");
#endif
        }
    }
}
