using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Repositories;

namespace TimeTracker.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;
        private readonly IMapper _mapper;

        public EmployeeService(IEmployeeRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(RegisterRequestDto dto)
        {
            var entity = _mapper.Map<ApplicationUser>(dto);
            var added = await _repo.AddAsync(entity);
            return _mapper.Map<EmployeeDto>(added);
        }

        public async Task<bool> DeleteEmployeeAsync(int id) =>
            await _repo.DeleteAsync(id);

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(u => _mapper.Map<EmployeeDto>(u));
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            return user is null
                ? null
                : _mapper.Map<EmployeeDto>(user);
        }

        public async Task<bool> UpdateEmployeeAsync(EmployeeDto dto)
        {
            var entity = _mapper.Map<ApplicationUser>(dto);
            return await _repo.UpdateAsync(entity);
        }
    }
}
