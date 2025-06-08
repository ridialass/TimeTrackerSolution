using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Core.Interfaces
{
    public interface ITimeEntryService
    {
        Task<IEnumerable<TimeEntryDto>> GetAllTimeEntriesAsync();
        Task<IEnumerable<TimeEntryDto>> GetTimeEntriesByEmployeeAsync(int employeeId);
        Task<TimeEntryDto> CreateTimeEntryAsync(TimeEntryDto newEntry);
        Task<TimeEntryDto?> GetTimeEntryByIdAsync(int id);
        Task<bool> DeleteTimeEntryAsync(int id);
        // … etc.
    }
}

