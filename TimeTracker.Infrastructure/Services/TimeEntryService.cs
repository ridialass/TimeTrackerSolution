using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Data;

namespace TimeTracker.Infrastructure.Services
{
    public class TimeEntryService : ITimeEntryService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public TimeEntryService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<TimeEntryDto> CreateTimeEntryAsync(TimeEntryDto newEntry)
        {
            var entity = _mapper.Map<TimeEntry>(newEntry);
            var inserted = await _db.TimeEntries.AddAsync(entity);
            await _db.SaveChangesAsync();
            return _mapper.Map<TimeEntryDto>(inserted.Entity);
        }

        public async Task<bool> DeleteTimeEntryAsync(int id)
        {
            var te = await _db.TimeEntries.FindAsync(id);
            if (te == null) return false;
            _db.TimeEntries.Remove(te);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TimeEntryDto>> GetAllTimeEntriesAsync()
        {
            var list = await _db.TimeEntries
                                .Include(te => te.User)
                                .OrderByDescending(te => te.StartTime)
                                .AsNoTracking()
                                .ToListAsync();
            return list.Select(te => _mapper.Map<TimeEntryDto>(te));
        }

        public async Task<TimeEntryDto?> GetTimeEntryByIdAsync(int id)
        {
            var te = await _db.TimeEntries
                              .Include(te => te.User)
                              .AsNoTracking()
                              .FirstOrDefaultAsync(x => x.Id == id);
            return te == null ? null : _mapper.Map<TimeEntryDto>(te);
        }

        public async Task<IEnumerable<TimeEntryDto>> GetTimeEntriesByEmployeeAsync(int employeeId)
        {
            var list = await _db.TimeEntries
                                .Where(te => te.UserId == employeeId)
                                .Include(te => te.User)
                                .OrderByDescending(te => te.StartTime)
                                .AsNoTracking()
                                .ToListAsync();
            return list.Select(te => _mapper.Map<TimeEntryDto>(te));
        }
    }
}