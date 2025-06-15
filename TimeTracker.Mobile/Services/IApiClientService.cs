// IApiClientService.cs
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Mobile.Services
{
    public interface IApiClientService
    {
        Task<LoginResponseDto> LoginAsync(string username, string password);
        Task RegisterAsync(RegisterRequestDto dto);
        Task<IEnumerable<EmployeeDto>> GetEmployeesAsync();
        Task<IEnumerable<TimeEntryDto>> GetTimeEntriesAsync(int userId);
        Task CreateTimeEntryAsync(TimeEntryDto entry);

        // si vous avez encore besoin des accès bas-niveau :
        Task<HttpResponseMessage> GetAsync(string requestUri);
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content);
    }
}
