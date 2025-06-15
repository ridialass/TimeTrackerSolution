using System.Collections.Generic;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Core.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<EmployeeDto> CreateEmployeeAsync(RegisterRequestDto dto);
        Task<bool> UpdateEmployeeAsync(EmployeeDto dto);
    }
}
