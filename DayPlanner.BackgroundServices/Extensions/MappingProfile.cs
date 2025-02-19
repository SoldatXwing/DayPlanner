using AutoMapper;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.FireStore.Models;
using FirebaseAdmin.Auth;

namespace DayPlanner.BackgroundServices.Extensions
{
    public class MappingProfile : Profile
    {   /// <summary>
        /// Initializes a new instance of the <see cref="MappingProfile"/> class.
        /// Configures mappings for <see cref="AppointmentRequest"/> to <see cref="Appointment"/>.
        /// </summary>
        public MappingProfile()
        {
            CreateMap<AppointmentRequest, Appointment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Start, opt => opt.MapFrom(src => src.Start.ToUniversalTime()))
                .ForMember(dest => dest.End, opt => opt.MapFrom(src => src.End.ToUniversalTime()));

            CreateMap<UserRecord, User>()
                .ForMember(dest => dest.LastSignInTimestamp, opt => opt.MapFrom(src => src.UserMetaData.LastSignInTimestamp));
            CreateMap<RegisterUserRequest, UserRecordArgs>();
            CreateMap<FirestoreAppointment, Appointment>();

            CreateMap<AppointmentRequest, FirestoreAppointment>()
                        .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow)) 
                        .ForMember(dest => dest.Id, opt => opt.Ignore())
                        .ForMember(dest => dest.UserId, opt => opt.Ignore())
                        .ForMember(dest => dest.Start, opt => opt.MapFrom(src => src.Start.ToUniversalTime())) 
                        .ForMember(dest => dest.End, opt => opt.MapFrom(src => src.End.ToUniversalTime()));
            CreateMap<FirestoreGoogleSyncToken, GoogleSyncToken>();

        }
    }
}
