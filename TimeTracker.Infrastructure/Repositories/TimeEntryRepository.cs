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
                .Include(te => te.Pauses)                 // ✅
                .Where(te => te.EndTime != null)
                .OrderByDescending(te => te.StartTime)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IEnumerable<TimeEntry>> GetByEmployeeAsync(int employeeId) =>
            await _db.TimeEntries
                .Where(te => te.UserId == employeeId && te.EndTime != null)
                .Include(te => te.User)
                .Include(te => te.Pauses)                 // ✅
                .OrderByDescending(te => te.StartTime)
                .AsNoTracking()
                .ToListAsync();

        public async Task<TimeEntry?> GetByIdAsync(int id) =>
            await _db.TimeEntries
                .Include(te => te.User)
                .Include(te => te.Pauses)                 // ✅
                .AsNoTracking()
                .FirstOrDefaultAsync(te => te.Id == id);

        public async Task<bool> UpdateAsync(TimeEntry detached)
        {
            // ⚠️ Ne pas faire Attach+Modified directement : la collection serait perdue
            var tracked = await _db.TimeEntries
                                   .Include(t => t.Pauses)
                                   .FirstOrDefaultAsync(t => t.Id == detached.Id);
            if (tracked == null) return false;

            // Scalars
            _db.Entry(tracked).CurrentValues.SetValues(detached);

            // Sync collection: stratégie simple = reset
            _db.PausePeriods.RemoveRange(tracked.Pauses);
            tracked.Pauses.Clear();

            if (detached.Pauses != null && detached.Pauses.Count > 0)
            {
                foreach (var p in detached.Pauses)
                {
                    p.Id = 0; // force insert propre
                    p.TimeEntryId = tracked.Id;
                }
                await _db.PausePeriods.AddRangeAsync(detached.Pauses);
            }

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
