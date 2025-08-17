// SECURITE :
// Ne pas afficher d'informations sensibles dans les boîtes de dialogue (messages d'erreur internes, stack traces, etc.).

#nullable enable
using System.Threading.Tasks;

namespace TimeTracker.Mobile.Services.Interfaces
{
    /// <summary>
    /// Abstraction des boîtes de dialogue côté mobile (MAUI).
    /// Implémentez ceci via Shell.Current.DisplayAlert/DisplayActionSheet/DisplayPrompt
    /// ou via une lib tierce (CommunityToolkit, etc.).
    /// </summary>
    public interface IDialogService
    {
        /// <summary>Affiche une alerte simple.</summary>
        Task ShowAlertAsync(string title, string message, string cancel = "OK");

        /// <summary>Affiche une confirmation (OK/Annuler). Retourne true si l'utilisateur accepte.</summary>
        Task<bool> ShowConfirmationAsync(string title, string message, string accept = "OK", string cancel = "Annuler");

        /// <summary>
        /// Affiche une feuille d'actions. Retourne le libellé du bouton choisi ou null si annulé.
        /// </summary>
        Task<string?> ShowActionSheetAsync(string title, string cancel, string? destruction = null, params string[] buttons);

        /// <summary>
        /// Affiche une invite de saisie (prompt). Retourne le texte entré ou null si annulé.
        /// </summary>
        Task<string?> ShowPromptAsync(
            string title,
            string message,
            string accept = "OK",
            string cancel = "Annuler",
            string? placeholder = null,
            int maxLength = -1,
            string? initialValue = null);

        /// <summary>Shortcut pour un message d'erreur cohérent.</summary>
        Task ShowErrorAsync(string message, string title = "Erreur");

        /// <summary>Shortcut pour un message de succès.</summary>
        Task ShowSuccessAsync(string message, string title = "Succès");
    }
}
