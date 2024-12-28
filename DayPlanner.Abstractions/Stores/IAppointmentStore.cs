using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;

namespace DayPlanner.Abstractions.Stores
{
    public interface IAppointmentStore
    {
        Task<List<Appointment>> GetUsersAppointments(string userId, DateTime start, DateTime end);
        Task DeleteAppointment(string appointmentId);
        Task<Appointment> GetAppointmentById(string appointmendId);
        Task<Appointment> UpdateAppointment(string appointmentId, AppointmentRequest request);
        Task<Appointment> CreateAppointment(AppointmentRequest request);
    }

}
