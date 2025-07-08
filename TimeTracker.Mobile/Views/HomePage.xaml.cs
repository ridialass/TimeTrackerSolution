using Microsoft.Maui.Controls;
using TimeTracker.Mobile.ViewModels;

namespace TimeTracker.Mobile.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage(HomeViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}