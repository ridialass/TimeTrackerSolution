using TimeTracker.Mobile.ViewModels;

namespace TimeTracker.Mobile.Views;

public partial class StartSessionPage : ContentPage
{
    public StartSessionPage(StartSessionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HomePage");
    }
}
