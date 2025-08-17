#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using TimeTracker.Mobile.Services.Interfaces;

namespace TimeTracker.Mobile.Services
{
    /// <summary>
    /// Implémentation MAUI de IDialogService.
    /// Appelle toujours l'UI thread et reste silencieux si aucune page n'est disponible.
    /// </summary>
    public class DialogService : IDialogService
    {
        public Task ShowAlertAsync(string title, string message, string cancel = "OK")
            => InvokeOnUI(async page => await page.DisplayAlert(
                string.IsNullOrWhiteSpace(title) ? "Info" : title,
                message ?? string.Empty,
                string.IsNullOrWhiteSpace(cancel) ? "OK" : cancel));

        public Task<bool> ShowConfirmationAsync(string title, string message, string accept = "OK", string cancel = "Annuler")
            => InvokeOnUI(async page => await page.DisplayAlert(
                string.IsNullOrWhiteSpace(title) ? "Confirmation" : title,
                message ?? string.Empty,
                string.IsNullOrWhiteSpace(accept) ? "OK" : accept,
                string.IsNullOrWhiteSpace(cancel) ? "Annuler" : cancel));

        public Task<string?> ShowActionSheetAsync(string title, string cancel, string? destruction = null, params string[] buttons)
            => InvokeOnUI(async page => await page.DisplayActionSheet(
                title ?? string.Empty,
                string.IsNullOrWhiteSpace(cancel) ? "Annuler" : cancel,
                string.IsNullOrWhiteSpace(destruction) ? null : destruction,
                buttons ?? System.Array.Empty<string>()));

        public Task<string?> ShowPromptAsync(
            string title,
            string message,
            string accept = "OK",
            string cancel = "Annuler",
            string? placeholder = null,
            int maxLength = -1,
            string? initialValue = null)
            => InvokeOnUI(async page => await page.DisplayPromptAsync(
                string.IsNullOrWhiteSpace(title) ? "Saisie" : title,
                message ?? string.Empty,
                string.IsNullOrWhiteSpace(accept) ? "OK" : accept,
                string.IsNullOrWhiteSpace(cancel) ? "Annuler" : cancel,
                placeholder,
                maxLength,
                keyboard: Keyboard.Default,
                initialValue));

        public Task ShowErrorAsync(string message, string title = "Erreur")
            => ShowAlertAsync(title, message, "OK");

        public Task ShowSuccessAsync(string message, string title = "Succès")
            => ShowAlertAsync(title, message, "OK");

        // -------- Helpers --------
        private static async Task<T?> InvokeOnUI<T>(Func<Page, Task<T>> action)
        {
            var page = Application.Current?.MainPage ?? Shell.Current?.CurrentPage;
            if (page is null) return default;
            return await MainThread.InvokeOnMainThreadAsync(() => action(page));
        }

        private static Task InvokeOnUI(Func<Page, Task> action)
        {
            var page = Application.Current?.MainPage ?? Shell.Current?.CurrentPage;
            if (page is null) return Task.CompletedTask;
            return MainThread.InvokeOnMainThreadAsync(() => action(page));
        }
    }
}
