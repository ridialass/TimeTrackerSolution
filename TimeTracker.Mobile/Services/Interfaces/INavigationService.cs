#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace TimeTracker.Mobile.Services.Interfaces
{
    /// <summary>
    /// Abstraction de la navigation (basée sur <see cref="Shell"/>).
    /// Implémentation typique : utiliser <c>Shell.Current.GoToAsync()</c> avec des routes nommées.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>Va à la page de connexion.</summary>
        Task GoToLoginPageAsync();

        /// <summary>Va à la page d’accueil (“Home”).</summary>
        Task GoToHomePageAsync();

        /// <summary>Va à la page de démarrage de session.</summary>
        Task GoToStartSessionPageAsync();

        /// <summary>Va à la page de fin de session.</summary>
        Task GoToEndSessionPageAsync();

        /// <summary>Va à la page listant les pointages.</summary>
        Task GoToTimeEntriesPageAsync();

        /// <summary>Revient à la page précédente.</summary>
        Task GoBackAsync();
    }
}