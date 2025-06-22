using TimeTracker.Mobile.ViewModels;

namespace TimeTracker.Mobile.Views;

public partial class TimeEntriesPage : ContentPage
{
    public TimeEntriesPage(TimeEntriesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}