using DayPlanner.Abstractions.Models.Backend;

namespace DayPlanner.Abstractions.Services
{
    public interface IAiService
    {
        Task<Appointment> GetSingleAppointment(string userMessage);
    }

}