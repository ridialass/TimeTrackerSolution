using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using TimeTracker.Mobile.Services;

namespace TimeTracker.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly ISessionStateService _sessionService;

    private string username = string.Empty;
    public string Username
    {
        get => username;
        set => SetProperty(ref username, value);
    }

    private string password = string.Empty;
    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }

    public LoginViewModel(ISessionStateService sessionService)
    {
        _sessionService = sessionService;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var success = await _sessionService.LoginAsync(username, password);
            if (!success)
                ErrorMessage = "Échec de la connexion : identifiants invalides ou erreur serveur.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors de la connexion : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}