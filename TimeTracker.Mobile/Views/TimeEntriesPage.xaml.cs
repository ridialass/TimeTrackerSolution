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
}
