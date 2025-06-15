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
    // Nouveau constructeur param�tre-less, utilis� par le XAML et si on fait `new LoginPage()` :
    public StartSessionPage()
        : this(
            // On r�cup�re le LoginViewModel dans le container MAUI
            Application.Current!
                           .Handler!
                           .MauiContext!
                           .Services
                           .GetRequiredService<StartSessionViewModel>()
          )
    {
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