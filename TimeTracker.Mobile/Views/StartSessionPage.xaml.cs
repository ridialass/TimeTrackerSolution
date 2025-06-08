namespace TimeTracker.Views;

public partial class StartSessionPage : ContentPage
{
	public StartSessionPage()
	{
		InitializeComponent();
	}
    private async void OnBackToHomeClicked(object sender, System.EventArgs e)
    {
        // Navigue vers HomePage (route enregistrée dans AppShell)
        await Shell.Current.GoToAsync(nameof(HomePage));
    }

    protected override bool OnBackButtonPressed()
    {
        // Plutôt que de bloquer absolument, on navigue explicitement vers HomePage
        Shell.Current.GoToAsync(nameof(HomePage));
        return true; // empêche le comportement par défaut (pop sans route valide)
    }
}