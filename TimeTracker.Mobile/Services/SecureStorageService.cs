using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace TimeTracker.Mobile.Services;

public class SecureStorageService : ISecureStorageService
{
    public Task SetAsync(string key, string value) => SecureStorage.SetAsync(key, value);

    public Task<string?> GetAsync(string key) => SecureStorage.GetAsync(key);

    public void Remove(string key) => SecureStorage.Remove(key);
}