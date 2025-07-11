using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;

namespace TimeTracker.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _db;
        public EmployeeRepository(ApplicationDbContext db) => _db = db;

        public async Task<ApplicationUser> AddAsync(ApplicationUser entity)
        {
            var entry = await _db.Users.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var emp = await _db.Users.FindAsync(id);
            if (emp == null) return false;
            _db.Users.Remove(emp);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync() =>
            await _db.Users
                     .AsNoTracking()
                     .ToListAsync();

        public async Task<ApplicationUser?> GetByIdAsync(int id) =>
            await _db.Users
                     .AsNoTracking()
                     .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<ApplicationUser?> GetByUsernameAsync(string username) =>
            await _db.Users
                     .AsNoTracking()
                     .FirstOrDefaultAsync(e => e.UserName == username);

        public async Task<bool> UpdateAsync(ApplicationUser entity)
        {
            if (!_db.Users.Local.Any(e => e.Id == entity.Id))
                _db.Users.Attach(entity);
            _db.Entry(entity).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<ApplicationUser> Items, int TotalCount)> GetPagedAsync(EmployeeQueryParameters query)
        {
            var employees = _db.Users.AsQueryable();

            // Filtrage
            if (!string.IsNullOrEmpty(query.FirstName))
                employees = employees.Where(e => e.FirstName != null && e.FirstName.Contains(query.FirstName));
            if (!string.IsNullOrEmpty(query.LastName))
                employees = employees.Where(e => e.LastName != null && e.LastName.Contains(query.LastName));
            if (!string.IsNullOrEmpty(query.Email))
                employees = employees.Where(e => e.Email != null && e.Email.Contains(query.Email));
            if (!string.IsNullOrEmpty(query.Town))
                employees = employees.Where(e => e.Town != null && e.Town.Contains(query.Town));
            if (!string.IsNullOrEmpty(query.Country))
                employees = employees.Where(e => e.Country != null && e.Country.Contains(query.Country));
            if (query.Role.HasValue)
                employees = employees.Where(e => e.Role == query.Role);

            var total = await employees.CountAsync();

            // Pagination
            int skip = (query.Page - 1) * query.PageSize;
            var results = await employees
                .OrderBy(e => e.Id)
                .Skip(skip)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return (results, total);
        }
    }
}