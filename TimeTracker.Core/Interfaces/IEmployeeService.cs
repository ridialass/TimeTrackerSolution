using System.Collections.Generic;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Core.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
        Task<(IEnumerable<EmployeeDto> Items, int TotalCount)> GetEmployeesPagedAsync(EmployeeQueryParameters query);
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<EmployeeDto> CreateEmployeeAsync(RegisterRequestDto dto);
        Task<bool> UpdateEmployeeAsync(UpdateEmployeeDto dto);
        Task<bool> PatchEmployeeAsync(int id, PatchEmployeeDto dto);
    }
}