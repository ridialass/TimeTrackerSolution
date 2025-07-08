using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace TimeTracker.Mobile.Services;

public class DialogService : IDialogService
{
    public Task ShowAlertAsync(string title, string message, string cancel) =>
        Application.Current.MainPage.DisplayAlert(title, message, cancel);
}