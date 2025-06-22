using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;

namespace TimeTracker.Mobile.ViewModels;

public partial class RegistrationViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string firstName = string.Empty;
    [ObservableProperty] private string lastName = string.Empty;
    [ObservableProperty] private string town = string.Empty;
    [ObservableProperty] private string country = string.Empty;
    [ObservableProperty] private string role = string.Empty;

    [ObservableProperty] private string registrationSuccess = string.Empty;
    // 🔥 REMOVED duplicate ObservableProperty for errorMessage

    public RegistrationViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    public async Task RegisterAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        RegistrationSuccess = string.Empty;

        try
        {
            if (!Enum.TryParse<UserRole>(Role, out var parsedRole))
                parsedRole = UserRole.Employee;

            var dto = new RegisterRequestDto
            {
                Username = Username,
                Password = Password,
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                Town = Town,
                Country = Country,
                Role = parsedRole
            };

            var result = await _authService.RegisterAsync(dto);
            if (result.IsSuccess)
                RegistrationSuccess = "Registration successful!";
            else
                ErrorMessage = result.Error ?? "Registration failed.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
