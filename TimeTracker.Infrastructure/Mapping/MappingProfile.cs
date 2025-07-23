using AutoMapper;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;

namespace TimeTracker.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ---- UTILISATEURS ----
            CreateMap<ApplicationUser, EmployeeDto>()
                .ForMember(d => d.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(d => d.Username, opt => opt.MapFrom(src => src.UserName));

            CreateMap<RegisterRequestDto, ApplicationUser>(MemberList.None)
                .ForMember(d => d.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(d => d.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(d => d.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(d => d.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(d => d.Town, opt => opt.MapFrom(src => src.Town))
                .ForMember(d => d.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(d => d.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.PasswordHash, opt => opt.Ignore())
                .ForMember(d => d.SecurityStamp, opt => opt.Ignore());

            CreateMap<EmployeeDto, ApplicationUser>(MemberList.None)
                .ForMember(d => d.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(d => d.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Email, opt => opt.Ignore())
                .ForMember(d => d.PasswordHash, opt => opt.Ignore())
                .ForMember(d => d.SecurityStamp, opt => opt.Ignore());

            // ---- PAUSES ----
            CreateMap<PausePeriod, PausePeriod>()
                .ForMember(d => d.Start, opt => opt.MapFrom(src => src.Start))
                .ForMember(d => d.End, opt => opt.MapFrom(src => src.End));

            // ---- TIME ENTRY ----
            CreateMap<TimeEntryDto, TimeEntry>()
                .ForMember(d => d.TravelDurationHours,
                    opt => opt.MapFrom(src =>
                        src.TravelTimeEstimate.HasValue
                            ? src.TravelTimeEstimate.Value.TotalHours
                            : (double?)null))
                .ForMember(d => d.Pauses, opt => opt.MapFrom(src => src.Pauses));

            CreateMap<TimeEntry, TimeEntryDto>()
                .ForMember(d => d.TravelTimeEstimate,
                    opt => opt.MapFrom(src =>
                        src.TravelDurationHours.HasValue
                            ? TimeSpan.FromHours(src.TravelDurationHours.Value)
                            : (TimeSpan?)null))
                .ForMember(d => d.Username,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(d => d.Pauses, opt => opt.MapFrom(src => src.Pauses));
        }
    }
}