using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;

namespace DayPlanner.Abstractions.Stores
{
    public interface IAppointmentStore
    {
        Task<List<Appointment>> GetUsersAppointments(string userId, DateTime start, DateTime end);
        Task<List<Appointment>> GetUsersAppointments(string userId);
        Task DeleteAppointment(string userId, string appointmentId);
        Task<Appointment?> GetAppointmentById(string appointmentId);
        Task<Appointment> UpdateAppointment(string appointmentId, AppointmentRequest request);
        Task<Appointment> CreateAppointment(string userId, AppointmentRequest request);
    }

}
