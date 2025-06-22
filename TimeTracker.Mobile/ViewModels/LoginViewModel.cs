using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using TimeTracker.Mobile.Services;

namespace TimeTracker.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    // ❌ This was duplicated from BaseViewModel
    // [ObservableProperty] private string errorMessage;
    // ✅ Use the one from BaseViewModel

    public LoginViewModel(IAuthService authService, INavigationService navigation)
    {
        _authService = authService;
        _navigation = navigation;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _authService.LoginAsync(Username, Password);
            if (result.IsSuccess)
            {
                await _navigation.GoToHomePageAsync();
            }
            else
            {
                ErrorMessage = "Login failed: " + (result.Error ?? "Unknown error");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
