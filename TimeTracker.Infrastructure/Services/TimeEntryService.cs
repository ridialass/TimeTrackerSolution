using AutoMapper;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Repositories;
using TimeTracker.Core.Resources;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            var entity = _mapper.Map<TimeEntry>(dto);
            var added = await _timeEntryRepo.AddAsync(entity);
            return _mapper.Map<TimeEntryDto>(added);
        }

        public async Task<bool> DeleteTimeEntryAsync(int id)
        {
            return await _timeEntryRepo.DeleteAsync(id);
        }

        public async Task<IEnumerable<TimeEntryDto>> GetAllTimeEntriesAsync()
        {
            var all = await _timeEntryRepo.GetAllAsync();
            return all.Select(e => _mapper.Map<TimeEntryDto>(e));
        }

        public async Task<IEnumerable<TimeEntryDto>> GetTimeEntriesByUserAsync(int userId)
        {
            var list = await _timeEntryRepo.GetByEmployeeAsync(userId);
            return list.Select(e => _mapper.Map<TimeEntryDto>(e));
        }

        public async Task<TimeEntryDto?> GetTimeEntryByIdAsync(int id)
        {
            var e = await _timeEntryRepo.GetByIdAsync(id);
            if (e == null)
                return null;

            return _mapper.Map<TimeEntryDto>(e);
        }

        public async Task<bool> UpdateTimeEntryAsync(TimeEntryDto dto)
        {
            var entity = _mapper.Map<TimeEntry>(dto);
            return await _timeEntryRepo.UpdateAsync(entity);
        }
    }
}
