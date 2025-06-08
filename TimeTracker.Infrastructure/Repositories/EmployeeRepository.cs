using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Data;

namespace TimeTracker.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        //        private readonly ApplicationDbContext _db;
        //        public EmployeeRepository(ApplicationDbContext db) => _db = db;

        //        public async Task<Employee> AddAsync(Employee entity)
        //        {
        //            var entry = await _db.Users.AddAsync(entity);
        //            await _db.SaveChangesAsync();
        //            return entry.Entity;
        //        }

        //        public async Task<bool> DeleteAsync(int id)
        //        {
        //            var emp = await _db.Users.FindAsync(id);
        //            if (emp == null) return false;
        //            _db.Users.Remove(emp);
        //            await _db.SaveChangesAsync();
        //            return true;
        //        }

        //        public async Task<IEnumerable<Employee>> GetAllAsync() =>
        //            await _db.Users.AsNoTracking().ToListAsync();

        //        public async Task<Employee?> GetByIdAsync(int id) =>
        //            await _db.Users.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

        //        public async Task<Employee?> GetByUsernameAsync(string username) =>
        //            await _db.Users.AsNoTracking().FirstOrDefaultAsync(e => e.Username == username);

        //        public async Task<bool> UpdateAsync(Employee entity)
        //        {
        //            if (!_db.Users.Local.Any(e => e.Id == entity.Id))
        //                _db.Users.Attach(entity);
        //            _db.Entry(entity).State = EntityState.Modified;
        //            await _db.SaveChangesAsync();
        //            return true;
        //        }
            }
    }
