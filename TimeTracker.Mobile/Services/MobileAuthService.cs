using TimeTracker.Core.DTOs;

namespace TimeTracker.Mobile.Services
{
    public class MobileAuthService
    {
        private readonly IApiClientService _apiClient;
        public LoginResponseDto? CurrentUser { get; private set; }

        public MobileAuthService(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var response = await _apiClient.LoginAsync(username, password);
            if (response == null) return false;

            CurrentUser = response;
            await SecureStorageService.SetJwtAsync(response.Token);
            return true;
        }

        public void Logout()
        {
            CurrentUser = null;
            SecureStorageService.RemoveJwt();
        }

        public async Task<bool> TryRestoreSessionAsync()
        {
            var jwt = await SecureStorageService.GetJwtAsync();
            if (jwt == null) return false;

            // Optionally, parse JWT to get user info, or call GET /api/employee/{id} to get fresh details
            // For simplicity, store just the token and rehydrate CurrentUser minimally
            CurrentUser = new LoginResponseDto { Token = jwt /*, maybe load EmployeeId from token */ };

            return true;
        }
    }
}
