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
    private string email = string.Empty;
    public string Email
    {
        get => email;
        set => SetProperty(ref email, value);
    }
    private string firstName = string.Empty;
    public string FirstName
    {
        get => firstName;
        set => SetProperty(ref firstName, value);
    }
    private string lastName = string.Empty;
    public string LastName
    {
        get => lastName;
        set => SetProperty(ref lastName, value);
    }
    private string town = string.Empty;
    public string Town
    {
        get => town;
        set => SetProperty(ref town, value);
    }
    private string country = string.Empty;
    public string Country
    {
        get => country;
        set => SetProperty(ref country, value);
    }
    private string role = string.Empty;
    public string Role
    {
        get => role;
        set => SetProperty(ref role, value);
    }

    private string registrationSuccess = string.Empty;
    public string RegistrationSuccess
    {
        get => registrationSuccess;
        set => SetProperty(ref registrationSuccess, value);
    }
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
        registrationSuccess = string.Empty;

        try
        {
            if (!Enum.TryParse<UserRole>(role, out var parsedRole))
                parsedRole = UserRole.Employee;

            var dto = new RegisterRequestDto
            {
                Username = username,
                Password = password,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Town = town,
                Country = country,
                Role = parsedRole
            };

            var result = await _authService.RegisterAsync(dto);
            if (result.IsSuccess)
                registrationSuccess = "Registration successful!";
            else
                ErrorMessage = result.Error ?? "Registration failed.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
