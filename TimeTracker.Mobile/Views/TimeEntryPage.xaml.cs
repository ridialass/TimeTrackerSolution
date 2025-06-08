namespace TimeTracker.Mobile.Views;

public partial class HistoryPage : ContentPage
{
	public HistoryPage()
	{
		InitializeComponent();
	}
    private async void OnBackToHomeClicked(object sender, System.EventArgs e)
    {
        // Navigue vers HomePage (route enregistr�e dans AppShell)
        await Shell.Current.GoToAsync(nameof(HomePage));
    }

    protected override bool OnBackButtonPressed()
    {
        // Plut�t que de bloquer absolument, on navigue explicitement vers HomePage
        Shell.Current.GoToAsync(nameof(HomePage));
        return true; // emp�che le comportement par d�faut (pop sans route valide)
    }
}