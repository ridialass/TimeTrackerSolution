using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;

namespace TimeTracker.Core.Interfaces
{   
    public interface ITimeEntryService
    {
        Task<IEnumerable<TimeEntryDto>> GetAllTimeEntriesAsync();
        Task<IEnumerable<TimeEntryDto>> GetTimeEntriesByUserAsync(int userId);
        Task<TimeEntryDto?> GetTimeEntryByIdAsync(int id);
        Task<TimeEntryDto> AddTimeEntryAsync(TimeEntryDto dto);
        Task<bool> DeleteTimeEntryAsync(int id);
    }

}

