// TimeTracker.Infrastructure/Mapping/ApplicationMappingProfile.cs
using AutoMapper;
using System;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;

namespace TimeTracker.Infrastructure.Mapping
{
    public class ApplicationMappingProfile : Profile
    {
        public ApplicationMappingProfile()
        {
            // ── UTILISATEURS ──────────────────────────────────────────────────────────

            // Pour renvoyer les infos d'un user à l'UI
            CreateMap<ApplicationUser, EmployeeDto>()
                // si votre EmployeeDto contient un champ Role de type string ou enum, mappez-le explicitement :
                .ForMember(d => d.Role, opt => opt.MapFrom(src => src.Role));

            // Pour créer un ApplicationUser à partir du dto de l'UI
            CreateMap<RegisterRequestDto, ApplicationUser>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(d => d.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(d => d.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(d => d.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(d => d.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(d => d.Town, opt => opt.MapFrom(src => src.Town))
                .ForMember(d => d.Country, opt => opt.MapFrom(src => src.Country));
                //// on ne touche pas aux champs Id, SecurityStamp, etc.
                //.ForAllOtherMembers(opt => opt.Ignore());

            // ── SESSIONS (TIME ENTRY) ──────────────────────────────────────────────────

            // Pour recevoir un POST depuis l'UI
            CreateMap<TimeEntryDto, TimeEntry>()
                // TravelDurationHours ← TravelTimeEstimate.TotalHours
                .ForMember(dest => dest.TravelDurationHours,
                    opt => opt.MapFrom(src =>
                        src.TravelTimeEstimate.HasValue
                            ? src.TravelTimeEstimate.Value.TotalHours
                            : (double?)null
                    ));

            // Pour renvoyer à l'UI après lecture en base
            CreateMap<TimeEntry, TimeEntryDto>()
                // TravelTimeEstimate ← TravelDurationHours → TimeSpan
                .ForMember(dest => dest.TravelTimeEstimate,
                    opt => opt.MapFrom(src =>
                        src.TravelDurationHours.HasValue
                            ? TimeSpan.FromHours(src.TravelDurationHours.Value)
                            : (TimeSpan?)null
                    ))
                // Remonter le UserName de l'utilisateur lié
                .ForMember(dest => dest.Username,
                    opt => opt.MapFrom(src => src.User.UserName));
        }
    }
}
