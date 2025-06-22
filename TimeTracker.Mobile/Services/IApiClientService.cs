using System.Collections.Generic;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Utils;

namespace TimeTracker.Mobile.Services;

public interface IApiClientService
{
    Task<Result<LoginResponseDto>> LoginAsync(string username, string password);
    Task<Result<bool>> RegisterAsync(RegisterRequestDto dto);
    Task<Result<IEnumerable<EmployeeDto>>> GetEmployeesAsync();
    Task<Result<IEnumerable<TimeEntryDto>>> GetTimeEntriesAsync(int userId);
    Task<Result<bool>> CreateTimeEntryAsync(TimeEntryDto entry);
}