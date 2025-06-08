using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Data;
using TimeTracker.Infrastructure.Repositories;

namespace TimeTracker.Infrastructure
{
    public static class InfrastructureModule
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
            // … if you have IAuthRepository, add that too

            return services;
        }
    }
}
