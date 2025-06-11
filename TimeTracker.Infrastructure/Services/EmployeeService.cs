using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public EmployeeService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            var list = await _db.Users.AsNoTracking().ToListAsync();
            return list.Select(e => _mapper.Map<EmployeeDto>(e));
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var e = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return e == null ? null : _mapper.Map<EmployeeDto>(e);
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var e = await _db.Users.FindAsync(id);
            if (e == null) return false;
            _db.Users.Remove(e);
            await _db.SaveChangesAsync();
            return true;
        }

        // … you could add UpdateEmployeeAsync() if needed
    }
}