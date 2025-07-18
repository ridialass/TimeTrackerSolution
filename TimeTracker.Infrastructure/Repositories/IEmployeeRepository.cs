﻿using System.Collections.Generic;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;

namespace TimeTracker.Infrastructure.Repositories
{
    public interface IEmployeeRepository
    {
        Task<ApplicationUser> AddAsync(ApplicationUser entity);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser?> GetByIdAsync(int id);
        Task<ApplicationUser?> GetByUsernameAsync(string username);
        Task<bool> UpdateAsync(ApplicationUser entity);

        Task<(IEnumerable<ApplicationUser> Items, int TotalCount)> GetPagedAsync(EmployeeQueryParameters query);
    }
}