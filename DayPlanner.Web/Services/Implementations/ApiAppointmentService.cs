using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Web.Refit;

namespace DayPlanner.Web.Services.Implementations
{
    internal class ApiAppointmentService(IDayPlannerApi api) : IAppointmentService
    {
        public async Task<List<Appointment>> GetAllAppointmentsInRangeAsync(DateTime start, DateTime end)
        {
            if(start > end)
                throw new ArgumentException("Start date must be before end date");
            if(end < start)
                throw new ArgumentException("End date must be after start date");
            var appointments = await api.GetAppointmentsByDateAsync(start, end);
            return appointments.ToList();
        }
    }
}
