using AutoMapper;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;

namespace TimeTracker.Infrastructure.Mapping
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            // Pour le GET des employés
            CreateMap<ApplicationUser, EmployeeDto>();

            // Si vous voulez automatiser aussi le register :
            CreateMap<RegisterRequestDto, EmployeeDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
