using DayPlanner.Abstractions.Models.Backend;

namespace DayPlanner.Abstractions.Services
{
    public interface IExternalAppointmentService
    {
        Task<List<Appointment>> GetAppointments(DateTime start, DateTime end);
    }

}