using AutoMapper;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;

namespace DayPlanner.Api.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AppointmentRequest, Appointment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id generated from Firestore
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Start, opt => opt.MapFrom(src => src.Start.ToUniversalTime()))
                .ForMember(dest => dest.End, opt => opt.MapFrom(src => src.End.ToUniversalTime()));
            ;

        }
    }
}
