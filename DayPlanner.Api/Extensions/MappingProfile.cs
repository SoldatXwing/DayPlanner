using AutoMapper;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;

namespace DayPlanner.Api.Extensions

{   /// <summary>
    /// Defines the mapping configuration for AutoMapper to map between request models and domain models.
    /// </summary>
    public class MappingProfile : Profile
    {   /// <summary>
        /// Initializes a new instance of the <see cref="MappingProfile"/> class.
        /// Configures mappings for <see cref="AppointmentRequest"/> to <see cref="Appointment"/>.
        /// </summary>
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
