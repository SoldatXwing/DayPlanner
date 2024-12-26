using DayPlanner.Abstractions.Models.Backend;

namespace DayPlanner.Abstractions.Stores
{
    public interface IAppointmentStore
    {
        Task<List<Appointment>> GetUsersAppointmentsAsync(string userId, DateTime start, DateTime end);
        Task DeleteUsersAppointmentsAsync(string appointmentId);
        Task<Appointment> GetSingleAppointmentAsync(string appointmentId);
    }
}
