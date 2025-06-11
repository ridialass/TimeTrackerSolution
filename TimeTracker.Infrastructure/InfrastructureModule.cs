// InfrastructureModule.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Interfaces;
//using TimeTracker.Infrastructure.Data;
using TimeTracker.Infrastructure.Repositories;
using TimeTracker.Infrastructure.Services;

namespace TimeTracker.Infrastructure
{
    public static class InfrastructureModule
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, string? connectionString)
        {
            // 1. DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 2. Repositories
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();

            // 3. Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<ITimeEntryService, TimeEntryService>();

            // 4. (Optional) AutoMapper profiles if you decide to use AutoMapper:
            // services.AddAutoMapper(typeof(InfrastructureModule).Assembly);

            return services;
        }
    }
}
