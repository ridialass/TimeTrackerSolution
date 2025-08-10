using AutoMapper;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;
using TimeTracker.Core.Helpers;
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

            var sessionTypeLabel = EnumLocalizationHelper.GetEnumLabel(resultDto.SessionType, _localizer);
            var dinnerPaidLabel = EnumLocalizationHelper.GetEnumLabel(resultDto.DinnerPaid, _localizer);

            Console.WriteLine($"SessionType localisé : {sessionTypeLabel}, DinnerPaid localisé : {dinnerPaidLabel}");

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
            var dtos = all.Select(e => {
                var dto = _mapper.Map<TimeEntryDto>(e);
                FillWorkDurationNet(dto);
                var sessionTypeLabel = EnumLocalizationHelper.GetEnumLabel(dto.SessionType, _localizer);
                var dinnerPaidLabel = EnumLocalizationHelper.GetEnumLabel(dto.DinnerPaid, _localizer);
                Console.WriteLine($"SessionType: {sessionTypeLabel}, DinnerPaid: {dinnerPaidLabel}");
                return dto;
            });
            return dtos;
        }

        public async Task<IEnumerable<TimeEntryDto>> GetTimeEntriesByUserAsync(int userId)
        {
            var employee = await _employeeRepo.GetByIdAsync(userId);
            if (employee == null)
                throw new Exception(_localizer["EmployeeNotFound"]);

            var list = await _timeEntryRepo.GetByEmployeeAsync(userId);
            var dtos = list.Select(e => {
                var dto = _mapper.Map<TimeEntryDto>(e);
                FillWorkDurationNet(dto);
                var sessionTypeLabel = EnumLocalizationHelper.GetEnumLabel(dto.SessionType, _localizer);
                var dinnerPaidLabel = EnumLocalizationHelper.GetEnumLabel(dto.DinnerPaid, _localizer);
                Console.WriteLine($"SessionType: {sessionTypeLabel}, DinnerPaid: {dinnerPaidLabel}");
                return dto;
            });
            return dtos;
        }

        public async Task<TimeEntryDto?> GetTimeEntryByIdAsync(int id)
        {
            var e = await _timeEntryRepo.GetByIdAsync(id);
            if (e == null)
                return null;
            var dto = _mapper.Map<TimeEntryDto>(e);
            var sessionTypeLabel = EnumLocalizationHelper.GetEnumLabel(dto.SessionType, _localizer);
            var dinnerPaidLabel = EnumLocalizationHelper.GetEnumLabel(dto.DinnerPaid, _localizer);
            FillWorkDurationNet(dto);
            Console.WriteLine($"SessionType: {sessionTypeLabel}, DinnerPaid: {dinnerPaidLabel}");
            return dto;
        }

        public async Task<bool> UpdateTimeEntryAsync(TimeEntryDto dto)
        {
            // Map complet (y compris Pauses)
            var entity = _mapper.Map<TimeEntry>(dto);
            return await _timeEntryRepo.UpdateAsync(entity);
            // Vérification de l'existence de l'entrée
            //var existing = await _timeEntryRepo.GetByIdAsync(dto.Id);
            //if (existing == null)
            //    return false;

            //existing.StartTime = dto.StartTime;
            //existing.EndTime = dto.EndTime;
            //existing.SessionType = dto.SessionType;
            //existing.DinnerPaid = dto.DinnerPaid;
            //existing.IncludesTravelTime = dto.IncludesTravelTime;
            //existing.StartAddress = dto.StartAddress;
            //existing.EndAddress = dto.EndAddress;
            //existing.TravelDurationHours = dto.TravelDurationHours;
            //existing.IsAdminModified = dto.IsAdminModified;

            //// MISE À JOUR DES PAUSES
            //existing.Pauses = dto.Pauses?.Select(p => new PausePeriod
            //{
            //    Start = p.Start,
            //    End = p.End
            //}).ToList() ?? new List<PausePeriod>();

            //return await _timeEntryRepo.UpdateAsync(existing);
        }

        // Methode pour recalculer la durée nette de travail
        private void FillWorkDurationNet(TimeEntryDto dto)
        {
            // Cas: pas de pause, ou pause non fermée ou incohérente
            var brut = dto.EndTime.HasValue ? dto.EndTime.Value - dto.StartTime : (TimeSpan?)null;
            var pause = TimeSpan.FromSeconds(dto.Pauses?.Where(p => p.End.HasValue)
                    .Sum(p => (p.End.Value - p.Start).TotalSeconds) ?? 0);

            dto.WorkDurationNet = (brut.HasValue && brut.Value > pause) ? brut - pause : brut;
        }
    }
}

