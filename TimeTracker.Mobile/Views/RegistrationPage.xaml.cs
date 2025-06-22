using TimeTracker.Mobile.ViewModels;

namespace TimeTracker.Mobile.Views;

public partial class RegistrationPage : ContentPage
{
    public RegistrationPage(RegistrationViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}