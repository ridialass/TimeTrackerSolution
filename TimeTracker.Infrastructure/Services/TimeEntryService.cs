using AutoMapper;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;
using TimeTracker.Core.Helpers; // Ajouté pour EnumLocalizationHelper
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Repositories;
using TimeTracker.Core.Resources;

namespace TimeTracker.Infrastructure.Services
{
    public class TimeEntryService : ITimeEntryService
    {
        private readonly ITimeEntryRepository _timeEntryRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<Errors> _localizer;

        public TimeEntryService(
            ITimeEntryRepository timeEntryRepo,
            IEmployeeRepository employeeRepo,
            IMapper mapper,
            IStringLocalizer<Errors> localizer)
        {
            _timeEntryRepo = timeEntryRepo;
            _employeeRepo = employeeRepo;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<TimeEntryDto> AddTimeEntryAsync(TimeEntryDto dto)
        {
            if (dto.UserId <= 0)
                throw new ArgumentException(_localizer["UserIdRequired"], nameof(dto.UserId));
            if (dto.StartTime == default || dto.EndTime == default)
                throw new ArgumentException(_localizer["TimePeriodRequired"], nameof(dto.StartTime));

            var entity = _mapper.Map<TimeEntry>(dto);
            var added = await _timeEntryRepo.AddAsync(entity);
            var resultDto = _mapper.Map<TimeEntryDto>(added);

            // Exemple d’utilisation d’EnumLocalizationHelper (usage interne, log, etc.)
            var sessionTypeLabel = EnumLocalizationHelper.GetEnumLabel(resultDto.SessionType, _localizer);
            var dinnerPaidLabel = EnumLocalizationHelper.GetEnumLabel(resultDto.DinnerPaid, _localizer);

            // Console.WriteLine($"SessionType localisé : {sessionTypeLabel}, DinnerPaid localisé : {dinnerPaidLabel}");

            return resultDto;
        }

        public async Task<bool> DeleteTimeEntryAsync(int id)
        {
            var deleted = await _timeEntryRepo.DeleteAsync(id);
            return deleted;
        }

        public async Task<IEnumerable<TimeEntryDto>> GetAllTimeEntriesAsync()
        {
            var all = await _timeEntryRepo.GetAllAsync();
            foreach (var e in all)
            {
                var dto = _mapper.Map<TimeEntryDto>(e);
                var sessionTypeLabel = EnumLocalizationHelper.GetEnumLabel(dto.SessionType, _localizer);
                var dinnerPaidLabel = EnumLocalizationHelper.GetEnumLabel(dto.DinnerPaid, _localizer);
                // Console.WriteLine($"SessionType: {sessionTypeLabel}, DinnerPaid: {dinnerPaidLabel}");
            }
            return all.Select(e => _mapper.Map<TimeEntryDto>(e));
        }

        public async Task<IEnumerable<TimeEntryDto>> GetTimeEntriesByUserAsync(int userId)
        {
            var employee = await _employeeRepo.GetByIdAsync(userId);
            if (employee == null)
                throw new Exception(_localizer["EmployeeNotFound"]);

            var list = await _timeEntryRepo.GetByEmployeeAsync(userId);
            foreach (var e in list)
            {
                var dto = _mapper.Map<TimeEntryDto>(e);
                var sessionTypeLabel = EnumLocalizationHelper.GetEnumLabel(dto.SessionType, _localizer);
                var dinnerPaidLabel = EnumLocalizationHelper.GetEnumLabel(dto.DinnerPaid, _localizer);
                // Console.WriteLine($"SessionType: {sessionTypeLabel}, DinnerPaid: {dinnerPaidLabel}");
            }
            return list.Select(e => _mapper.Map<TimeEntryDto>(e));
        }

        public async Task<TimeEntryDto?> GetTimeEntryByIdAsync(int id)
        {
            var e = await _timeEntryRepo.GetByIdAsync(id);
            if (e == null)
                return null;
            var dto = _mapper.Map<TimeEntryDto>(e);
            var sessionTypeLabel = EnumLocalizationHelper.GetEnumLabel(dto.SessionType, _localizer);
            var dinnerPaidLabel = EnumLocalizationHelper.GetEnumLabel(dto.DinnerPaid, _localizer);
            // Console.WriteLine($"SessionType: {sessionTypeLabel}, DinnerPaid: {dinnerPaidLabel}");
            return dto;
        }

        public async Task<bool> UpdateTimeEntryAsync(TimeEntryDto dto)
        {
            var existing = await _timeEntryRepo.GetByIdAsync(dto.Id);
            if (existing == null)
                return false;

            var entity = _mapper.Map<TimeEntry>(dto);
            return await _timeEntryRepo.UpdateAsync(entity);
        }
    }
}