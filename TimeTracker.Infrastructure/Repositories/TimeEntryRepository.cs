using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Entities;

namespace TimeTracker.Infrastructure.Repositories
{
    public class TimeEntryRepository : ITimeEntryRepository
    {
        private readonly ApplicationDbContext _db;
        public TimeEntryRepository(ApplicationDbContext db) => _db = db;

        public async Task<TimeEntry> AddAsync(TimeEntry entity)
        {
            var entry = await _db.TimeEntries.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var te = await _db.TimeEntries.FindAsync(id);
            if (te == null) return false;
            _db.TimeEntries.Remove(te);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TimeEntry>> GetAllAsync() =>
            await _db.TimeEntries
                     .Include(te => te.User)
                     .OrderByDescending(te => te.StartTime)
                     .AsNoTracking()
                     .ToListAsync();

        public async Task<IEnumerable<TimeEntry>> GetByEmployeeAsync(int employeeId) =>
            await _db.TimeEntries
                     .Where(te => te.UserId == employeeId)
                     .Include(te => te.User)
                     .OrderByDescending(te => te.StartTime)
                     .AsNoTracking()
                     .ToListAsync();

        public async Task<TimeEntry?> GetByIdAsync(int id) =>
            await _db.TimeEntries
                     .Include(te => te.User)
                     .AsNoTracking()
                     .FirstOrDefaultAsync(te => te.Id == id);

        public async Task<bool> UpdateAsync(TimeEntry entity)
        {
            if (!_db.TimeEntries.Local.Any(e => e.Id == entity.Id))
                _db.TimeEntries.Attach(entity);
            _db.Entry(entity).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
