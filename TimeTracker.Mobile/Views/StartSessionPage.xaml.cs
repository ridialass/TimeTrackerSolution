using TimeTracker.Mobile.ViewModels;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile.Views;

public partial class StartSessionPage : ContentPage
{
	public StartSessionPage(StartSessionViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }

    private async void OnBackToHomeClicked(object sender, System.EventArgs e)
    {
        await Shell.Current.GoToAsync("///HomePage");
    }

}