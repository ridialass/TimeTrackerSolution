// TimeTracker.Infrastructure/Mapping/MappingProfile.cs
using System;
using AutoMapper;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;

namespace TimeTracker.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // -------------------- UTILISATEURS --------------------
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

            // -------------------- PROFILE --------------------
            CreateMap<ApplicationUser, UserProfileDto>()
                .ForMember(d => d.Email, opt => opt.MapFrom(src => src.Email ?? string.Empty))
                .ForMember(d => d.FirstName, opt => opt.MapFrom(src => src.FirstName ?? string.Empty))
                .ForMember(d => d.LastName, opt => opt.MapFrom(src => src.LastName ?? string.Empty))
                .ForMember(d => d.Town, opt => opt.MapFrom(src => src.Town ?? string.Empty))
                .ForMember(d => d.Country, opt => opt.MapFrom(src => src.Country ?? string.Empty))
                .ForMember(d => d.ProfilePictureUrl, opt => opt.MapFrom(src => src.ProfilePictureUrl ?? string.Empty));

            CreateMap<UpdateProfileDto, ApplicationUser>()
                .ForMember(d => d.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(d => d.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(d => d.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(d => d.Town, opt => opt.MapFrom(src => src.Town))
                .ForMember(d => d.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(d => d.ProfilePictureUrl, opt => opt.MapFrom(src => src.ProfilePictureUrl));

            // -------------------- PAUSES --------------------
            CreateMap<PausePeriodDto, PausePeriod>().ReverseMap();

            // -------------------- TIME ENTRY --------------------
            // DTO -> Entity
            CreateMap<TimeEntryDto, TimeEntry>()
                // Si TravelTimeEstimate est fourni, on l'utilise ; sinon on conserve TravelDurationHours éventuel du DTO
                .ForMember(d => d.TravelDurationHours, opt => opt.MapFrom(src =>
                    src.TravelTimeEstimate.HasValue
                        ? (double?)src.TravelTimeEstimate.Value.TotalHours
                        : src.TravelDurationHours))
                // Ne pas toucher aux pauses si le client n'a pas envoyé la collection (mise à jour partielle)
                .ForMember(d => d.Pauses, opt =>
                {
                    opt.PreCondition(src => src.Pauses != null);
                    opt.MapFrom(src => src.Pauses!);
                })
                // Évite d'écraser des champs avec null lors d'updates partielles
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Entity -> DTO
            CreateMap<TimeEntry, TimeEntryDto>()
                .ForMember(d => d.TravelTimeEstimate, opt => opt.MapFrom(src =>
                    src.TravelDurationHours.HasValue
                        ? TimeSpan.FromHours(src.TravelDurationHours.Value)
                        : (TimeSpan?)null))
                .ForMember(d => d.Username, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(d => d.Pauses, opt => opt.MapFrom(src => src.Pauses));
        }
    }
}
