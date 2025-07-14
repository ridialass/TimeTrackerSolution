using AutoMapper;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;
using TimeTracker.Core.Helpers; // Pour le helper, si besoin d'utiliser EnumLocalizationHelper
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Repositories;
using TimeTracker.Core.Resources;

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
            if (string.IsNullOrWhiteSpace(dto.Username))
                throw new ArgumentException(_localizer["UsernameRequired"], nameof(dto.Username));
            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException(_localizer["PasswordRequired"], nameof(dto.Password));
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException(_localizer["EmailRequired"], nameof(dto.Email));

            var entity = _mapper.Map<ApplicationUser>(dto);
            var added = await _repo.AddAsync(entity);
            var emp = _mapper.Map<EmployeeDto>(added);

            // Utilisation interne du label localisé :
            var roleLabel = EnumLocalizationHelper.GetEnumLabel(emp.Role, _localizer);
            // Exemple : log ou usage interne
            // Console.WriteLine($"Rôle (localisé) : {roleLabel}");

            return emp;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var deleted = await _repo.DeleteAsync(id);
            return deleted;
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            var list = await _repo.GetAllAsync();
            // Utilisation interne du helper, exemple pour le premier rôle
            foreach (var user in list)
            {
                var emp = _mapper.Map<EmployeeDto>(user);
                Console.WriteLine($"Emp.UserName: {user.UserName} | DTO.Username: {emp.Username}");
                var roleLabel = EnumLocalizationHelper.GetEnumLabel(emp.Role, _localizer);
                // Console.WriteLine($"Employé {emp.Username}, rôle : {roleLabel}");
            }
            return list.Select(u => _mapper.Map<EmployeeDto>(u));

        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user is null)
                return null;
            var emp = _mapper.Map<EmployeeDto>(user);

            var roleLabel = EnumLocalizationHelper.GetEnumLabel(emp.Role, _localizer);
            // Console.WriteLine($"Rôle employé : {roleLabel}");

            return emp;
        }

        public async Task<bool> UpdateEmployeeAsync(UpdateEmployeeDto dto)
        {
            var existing = await _repo.GetByIdAsync(dto.Id);
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
            // Utilisation interne du helper pour chaque employé
            foreach (var user in results)
            {
                var emp = _mapper.Map<EmployeeDto>(user);
                var roleLabel = EnumLocalizationHelper.GetEnumLabel(emp.Role, _localizer);
                // Console.WriteLine($"Employé paginé {emp.Username}, rôle : {roleLabel}");
            }
            return (results.Select(u => _mapper.Map<EmployeeDto>(u)), total);
        }
    }
}