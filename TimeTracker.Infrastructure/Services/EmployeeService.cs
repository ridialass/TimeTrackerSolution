using AutoMapper;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Repositories;
using TimeTracker.Core.Resources;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TimeTracker.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<Errors> _localizer;

        public EmployeeService(IEmployeeRepository repo, 
            IMapper mapper, 
            IStringLocalizer<Errors> localizer)
        {
            _repo = repo;
            _mapper = mapper;
            _localizer = localizer;
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

        // Mise à jour complète (PUT)
        public async Task<bool> UpdateEmployeeAsync(UpdateEmployeeDto dto)
        {
            var existing = await _repo.GetByIdAsync(dto.Id);
            if (existing == null)
                return false;

            // Mettre à jour les champs principaux
            if (dto.FirstName != null)
                existing.FirstName = dto.FirstName;
            if (dto.LastName != null)
                existing.LastName = dto.LastName;
            if (dto.Email != null)
                existing.Email = dto.Email;
            if (dto.Town != null)
                existing.Town = dto.Town;
            if (dto.Country != null)
                existing.Country = dto.Country;
            if (dto.Role.HasValue)
                existing.Role = dto.Role.Value;

            return await _repo.UpdateAsync(existing);
        }

        // Mise à jour partielle (PATCH)
        public async Task<bool> PatchEmployeeAsync(int id, PatchEmployeeDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return false;

            if (dto.FirstName != null)
                existing.FirstName = dto.FirstName;
            if (dto.LastName != null)
                existing.LastName = dto.LastName;
            if (dto.Email != null)
                existing.Email = dto.Email;
            if (dto.Town != null)
                existing.Town = dto.Town;
            if (dto.Country != null)
                existing.Country = dto.Country;
            if (dto.Role.HasValue)
                existing.Role = dto.Role.Value;

            return await _repo.UpdateAsync(existing);
        }
        public async Task<(IEnumerable<EmployeeDto> Items, int TotalCount)> GetEmployeesPagedAsync(EmployeeQueryParameters query)
        {
            var (results, total) = await _repo.GetPagedAsync(query);
            return (results.Select(u => _mapper.Map<EmployeeDto>(u)), total);
        }
    }
}