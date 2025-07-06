using TimeTracker.Mobile.ViewModels;

namespace TimeTracker.Mobile.Views;

public partial class TimeEntriesPage : ContentPage
{
    private TimeEntriesViewModel? ViewModel => BindingContext as TimeEntriesViewModel;

    public TimeEntriesPage(TimeEntriesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (ViewModel != null)
            await ViewModel.LoadTimeEntriesAsync();
    }
    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(HomePage));
    }
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        if (Application.Current is App app)
            await app.LogoutAsync();
    }
}
