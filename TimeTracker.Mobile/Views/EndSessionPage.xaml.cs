using TimeTracker.Mobile.ViewModels;

namespace TimeTracker.Mobile.Views;

public partial class EndSessionPage : ContentPage
{
    public EndSessionPage(EndSessionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
    // Nouveau constructeur paramètre-less, utilisé par le XAML et si on fait `new LoginPage()` :
    //public EndSessionPage()
    //    : this(
    //        // On récupère le LoginViewModel dans le container MAUI
    //        Application.Current!
    //                       .Handler!
    //                       .MauiContext!
    //                       .Services
    //                       .GetRequiredService<EndSessionViewModel>()
    //      )
    //{
    //}
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