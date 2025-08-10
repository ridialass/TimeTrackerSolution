// TimeTracker.Mobile/Services/ISecureStorageService.cs
#nullable enable
using System.Threading.Tasks;

namespace TimeTracker.Mobile.Services
{
    public interface ISecureStorageService
    {
        Task SetAsync(string key, string value);
        Task<string?> GetAsync(string key);
        void Remove(string key);
        Task RemoveAsync(string key);
    }
}
