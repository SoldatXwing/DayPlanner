using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Refit;

namespace DayPlanner.Web.Services.Implementations
{
    internal class ApiAppointmentService(IDayPlannerApi api) : IAppointmentService
    {
        public async Task<Appointment> CreateAppointmentAsync(AppointmentRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            return await api.CreateAppointmentAsync(request);
        }

        public async Task DeleteAppointmentAsync(string appointmentId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(appointmentId);
            await api.DeleteAppointmentAsync(appointmentId);
        }

        public async Task<List<Appointment>> GetAllAppointmentsInRangeAsync(DateTime start, DateTime end)
        {
            if(start > end)
                throw new ArgumentException("Start date must be before end date");
            if(end < start)
                throw new ArgumentException("End date must be after start date");
            var appointments = await api.GetAppointmentsByDateAsync(start, end);
            return [.. appointments];
        }

        public async Task<Appointment> UpdateAppointmentAsync(string appointmentId, AppointmentRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrEmpty(appointmentId);
            return await api.UpdateAppointmentAsync(appointmentId,request);
        }
    }
}
