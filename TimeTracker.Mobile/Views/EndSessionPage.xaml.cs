using TimeTracker.Mobile.ViewModels;

namespace TimeTracker.Mobile.Views;

public partial class EndSessionPage : ContentPage
{
    private EndSessionViewModel Vm => (EndSessionViewModel)BindingContext;

    public EndSessionPage(EndSessionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Vm.ReloadSessionAsync();
    }

    private async void OnBackToHomeClicked(object sender, System.EventArgs e)
    {
        await Shell.Current.GoToAsync("///HomePage");
    }

    
}