#nullable enable
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class BaseViewModel : ObservableObject
{
    // ---- Busy state ----
    private bool isBusy;
    public bool IsBusy
    {
        get => isBusy;
        set
        {
            if (SetProperty(ref isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }
    }

    /// <summary>Inverse de <see cref="IsBusy"/> pour du binding pratique.</summary>
    public bool IsNotBusy => !IsBusy;

    // ---- Error state ----
    private string errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => errorMessage;
        set
        {
            if (SetProperty(ref errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    /// <summary>True si un message d’erreur est présent.</summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    // ---- Optionnel : titre (pour entêtes de pages) ----
    private string? title;
    public string? Title
    {
        get => title;
        set => SetProperty(ref title, value);
    }

    // ---- Cycle de vie simple ----
    private bool isInitialized;
    public bool IsInitialized
    {
        get => isInitialized;
        protected set => SetProperty(ref isInitialized, value);
    }

    /// <summary>
    /// Méthode d'initialisation (appelée une fois). Appelle <see cref="OnInitializeAsync"/>.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (IsInitialized) return;
        await RunBusyAsync(async () =>
        {
            await OnInitializeAsync();
            IsInitialized = true;
        });
    }

    /// <summary>
    /// À surcharger dans les ViewModels concrets pour charger les données initiales.
    /// </summary>
    protected virtual Task OnInitializeAsync() => Task.CompletedTask;

    // ---- Helpers ----

    /// <summary>
    /// Exécute une action asynchrone en gérant IsBusy/erreur de manière centralisée.
    /// </summary>
    /// <param name="action">Le travail à exécuter.</param>
    /// <param name="onError">Callback optionnel sur exception.</param>
    /// <param name="userFriendlyError">
    /// Message à afficher si une erreur survient (sinon message générique).
    /// </param>
    protected async Task RunBusyAsync(
        Func<Task> action,
        Action<Exception>? onError = null,
        string? userFriendlyError = null)
    {
        if (IsBusy) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM] Error: {ex}");
            ErrorMessage = string.IsNullOrWhiteSpace(userFriendlyError)
                ? "Une erreur est survenue. Veuillez réessayer."
                : userFriendlyError;
            onError?.Invoke(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Réinitialise l’état d’erreur.</summary>
    protected void ClearError() => ErrorMessage = string.Empty;
}