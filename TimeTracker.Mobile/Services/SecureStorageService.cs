using Microsoft.Maui.Storage;

namespace TimeTracker.Mobile.Services
{
    public static class SecureStorageService
    {
        private const string JwtKey = "JwtToken";
        public static Task SetJwtAsync(string token) =>
            SecureStorage.SetAsync(JwtKey, token);

        public static Task<string?> GetJwtAsync() =>
            SecureStorage.GetAsync(JwtKey);

        public static void RemoveJwt() =>
            SecureStorage.Remove(JwtKey);
    }
}
