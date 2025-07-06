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
        // Navigue vers HomePage (route enregistrée dans AppShell)
        await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
    }

    protected override bool OnBackButtonPressed()
    {
        // Plutôt que de bloquer absolument, on navigue explicitement vers HomePage
        Shell.Current.GoToAsync(nameof(HomePage));
        return true; // empêche le comportement par défaut (pop sans route valide)
    }
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        if (Application.Current is App app)
            await app.LogoutAsync();
    }
}