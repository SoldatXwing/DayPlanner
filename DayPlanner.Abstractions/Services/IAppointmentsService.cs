using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Stores;

namespace DayPlanner.Abstractions.Services
{
    public interface IAppointmentsService
    {
        Task<List<Appointment>> GetUsersAppointments(string userId, DateTime start, DateTime end);
        Task<List<Appointment>> GetUsersAppointments(string userId);
        Task DeleteUsersAppointment(string userId, string appointmentId);
        Task<Appointment?> GetAppointmentById(string appointmendId);
        Task<Appointment> UpdateAppointment(string appointmentId, AppointmentRequest request);
        Task<Appointment> CreateAppointment(string userId, AppointmentRequest request);
    }
    public class AppointmentsService : IAppointmentsService
    {
        private readonly IAppointmentStore _appointmentStore;

        public AppointmentsService(IAppointmentStore appointmentStore) => _appointmentStore = appointmentStore;

        public async Task<Appointment> CreateAppointment(string userId, AppointmentRequest request) => request is null ? throw new ArgumentNullException(nameof(request), "Appointment cant be null.") : await _appointmentStore.CreateAppointment(userId,request);
        public async Task DeleteUsersAppointment(string userId, string appointmentId)
        {
            if (string.IsNullOrEmpty(appointmentId))
                throw new ArgumentNullException(nameof(appointmentId), "Appointment Id cant be null or empty.");
            await _appointmentStore.DeleteAppointment(userId, appointmentId);
        }

        public async Task<Appointment?> GetAppointmentById(string appointmendId) => string.IsNullOrEmpty(appointmendId) ? throw new ArgumentNullException(nameof(appointmendId), "Appointment Id cant be null or empty.") : await _appointmentStore.GetAppointmentById(appointmendId);

        public async Task<List<Appointment>> GetUsersAppointments(string userId, DateTime start, DateTime end)
        {
            if(start > end)
                throw new ArgumentException("Start date cant be greater than end date.");
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "User Id cant be null or empty.");
            return await _appointmentStore.GetUsersAppointments(userId, start, end);
        }
        public async Task<List<Appointment>> GetUsersAppointments(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "User Id cant be null or empty.");
            return await _appointmentStore.GetUsersAppointments(userId);
        }

        public async Task<Appointment> UpdateAppointment(string appointmentId, AppointmentRequest request)
        {
            if(string.IsNullOrEmpty(appointmentId))
                throw new ArgumentNullException(nameof(appointmentId), "Appointment Id cant be null or empty.");
            if (request is null)
                throw new ArgumentNullException(nameof(appointmentId), "Appointment cant be null.");
            return await _appointmentStore.UpdateAppointment(appointmentId, request);

        }
    }
}