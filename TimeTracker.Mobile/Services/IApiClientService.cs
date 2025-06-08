using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Mobile.Services
{
    public interface IApiClientService
    {
        Task<LoginResponseDto?> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(EmployeeDto newEmployee, string password);
        Task<List<EmployeeDto>> GetAllEmployeesAsync(string jwtToken);
        Task<List<TimeEntryDto>> GetTimeEntriesByEmployeeAsync(int employeeId, string jwtToken);
        Task<TimeEntryDto> CreateTimeEntryAsync(TimeEntryDto payload, string jwtToken);
    }
}
