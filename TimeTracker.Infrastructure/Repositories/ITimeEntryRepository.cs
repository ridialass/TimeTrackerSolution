using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;

namespace TimeTracker.Infrastructure.Repositories
{
    public interface ITimeEntryRepository
    {
        Task<TimeEntry> AddAsync(TimeEntry entity);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TimeEntry>> GetAllAsync();
        Task<IEnumerable<TimeEntry>> GetByEmployeeAsync(int employeeId);
        Task<TimeEntry?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(TimeEntry entity);
    }
}
