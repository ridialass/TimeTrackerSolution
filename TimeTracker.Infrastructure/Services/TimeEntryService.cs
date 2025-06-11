// Services/TimeEntryService.cs
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Infrastructure.Repositories;

namespace TimeTracker.Infrastructure.Services
{
    public class TimeEntryService : ITimeEntryService
    {
        private readonly ITimeEntryRepository _timeEntryRepo;
        private readonly IEmployeeRepository _employeeRepo;

        //public TimeEntryService(
        //    ITimeEntryRepository timeEntryRepo,
        //    IEmployeeRepository employeeRepo)
        //{
        //    _timeEntryRepo = timeEntryRepo;
        //    _employeeRepo = employeeRepo;
        //}

        //public async Task<TimeEntryDto> ClockInAsync(Guid employeeId, CancellationToken cancellationToken)
        //{
        //    // 1) Ensure employee exists:
        //    var employee = await _employeeRepo.GetByIdAsync(employeeId, cancellationToken);
        //    if (employee == null)
        //        throw new KeyNotFoundException("Employee not found");

        //    // 2) Ensure there’s no open entry for today
        //    var open = await _timeEntryRepo.GetOpenEntryForEmployeeAsync(employeeId, cancellationToken);
        //    if (open != null)
        //        throw new InvalidOperationException("You’re already clocked in.");

        //    var newEntry = new TimeEntry
        //    {
        //        Id = Guid.NewGuid(),
        //        EmployeeId = employeeId,
        //        ClockIn = DateTime.UtcNow,
        //        ClockOut = null
        //    };
        //    var saved = await _timeEntryRepo.AddAsync(newEntry, cancellationToken);
        //    return new TimeEntryDto
        //    {
        //        Id = saved.Id,
        //        EmployeeId = saved.EmployeeId,
        //        ClockIn = saved.ClockIn,
        //        ClockOut = saved.ClockOut
        //    };
        //}

        //public async Task<TimeEntryDto> ClockOutAsync(Guid employeeId, CancellationToken cancellationToken)
        //{
        //    // 1) Find the open entry
        //    var open = await _timeEntryRepo.GetOpenEntryForEmployeeAsync(employeeId, cancellationToken);
        //    if (open == null)
        //        throw new InvalidOperationException("No open time entry to clock out.");

        //    open.ClockOut = DateTime.UtcNow;
        //    var updated = await _timeEntryRepo.UpdateAsync(open, cancellationToken);

        //    return new TimeEntryDto
        //    {
        //        Id = updated.Id,
        //        EmployeeId = updated.EmployeeId,
        //        ClockIn = updated.ClockIn,
        //        ClockOut = updated.ClockOut
        //    };
        //}

        //public async Task<IEnumerable<TimeEntryDto>> GetTimeEntriesForEmployeeAsync(
        //    Guid employeeId, DateTime? from, DateTime? to, CancellationToken cancellationToken)
        //{
        //    var entries = await _timeEntryRepo.GetEntriesForEmployeeAsync(employeeId, from, to, cancellationToken);
        //    return entries.Select(e => new TimeEntryDto
        //    {
        //        Id = e.Id,
        //        EmployeeId = e.EmployeeId,
        //        ClockIn = e.ClockIn,
        //        ClockOut = e.ClockOut
        //    });
        //}

        Task ITimeEntryService.AddTimeEntryAsync(TimeEntry entity)
        {
            throw new NotImplementedException();
        }

        Task<bool> ITimeEntryService.DeleteTimeEntryAsync(int id)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<TimeEntryDto>> ITimeEntryService.GetAllTimeEntriesAsync()
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<TimeEntryDto>> ITimeEntryService.GetTimeEntriesByUserAsync(int userId)
        {
            throw new NotImplementedException();
        }

        Task<TimeEntryDto> ITimeEntryService.GetTimeEntryByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}