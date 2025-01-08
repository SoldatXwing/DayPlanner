using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Stores;

namespace DayPlanner.Abstractions.Services
{
    public interface IAppointmentsService
    {
        Task<List<Appointment>> GetUsersAppointments(string userId, DateTime start, DateTime end);

        Task<long?> GetAppointmentsCount(string userId);

        Task<List<Appointment>> GetUsersAppointments(string userId, int page, int pageSize);

        Task DeleteUsersAppointment(string userId, string appointmentId);

        Task<Appointment?> GetAppointmentById(string userId,string appointmendId);

        Task<Appointment> UpdateAppointment(string appointmentId, string userId, AppointmentRequest request);

        Task<Appointment> CreateAppointment(string userId, AppointmentRequest request);
        Task ImportOrUpdateAppointments(string userId, List<Appointment> appointments);
    }

    public class AppointmentsService : IAppointmentsService
    {
        private readonly IAppointmentStore _appointmentStore;

        public AppointmentsService(IAppointmentStore appointmentStore) => _appointmentStore = appointmentStore;

        public async Task<Appointment> CreateAppointment(string userId, AppointmentRequest request) => request is null ? throw new ArgumentNullException(nameof(request), "Appointment cant be null.") : await _appointmentStore.CreateAppointment(userId,request);

        public async Task ImportOrUpdateAppointments(string userId, List<Appointment> appointments)
        {
            foreach (var appointment in appointments)
            {
                if (appointment is null)
                    throw new ArgumentNullException(nameof(appointment), "Appointment cant be null.");
                var existingAppointment = await _appointmentStore.GetAppointmentById(userId, appointment.Id);
                if (existingAppointment is not null)
                {
                    await _appointmentStore.UpdateAppointment(appointment.Id, userId, new AppointmentRequest
                    {
                        Summary = appointment.Summary,
                        Start = appointment.Start,
                        End = appointment.End,
                        Location = appointment.Location,
                        Title = appointment.Title,
                    });
                }
                else
                {
                    await _appointmentStore.ImportAppointment(userId,appointment.Id, new AppointmentRequest
                    {
                        Summary = appointment.Summary,
                        Start = appointment.Start,
                        End = appointment.End,
                        Location = appointment.Location,
                        Title = appointment.Title,
                    });
                }
            }
        }

        public async Task DeleteUsersAppointment(string userId, string appointmentId)
        {
            if (string.IsNullOrEmpty(appointmentId))
                throw new ArgumentNullException(nameof(appointmentId), "Appointment Id cant be null or empty.");
            await _appointmentStore.DeleteAppointment(userId, appointmentId);
        }

        public async Task<Appointment?> GetAppointmentById(string userId,string appointmendId) => string.IsNullOrEmpty(appointmendId) ? throw new ArgumentNullException(nameof(appointmendId), "Appointment Id cant be null or empty.") : await _appointmentStore.GetAppointmentById(userId,appointmendId);

        public async Task<long?> GetAppointmentsCount(string userId) => string.IsNullOrEmpty(userId) ? throw new ArgumentNullException(nameof(userId), "User id cant be null or empty.") : await _appointmentStore.GetAppointmentsCount(userId);

        public async Task<List<Appointment>> GetUsersAppointments(string userId, DateTime start, DateTime end)
        {
            if(start > end)
                throw new ArgumentException("Start date cant be greater than end date.");
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "User Id cant be null or empty.");
            return await _appointmentStore.GetUsersAppointments(userId, start, end);
        }
        public async Task<List<Appointment>> GetUsersAppointments(string userId, int page, int pageSize)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "User Id cant be null or empty.");
            return await _appointmentStore.GetUsersAppointments(userId, page, pageSize);
        }

        public async Task<Appointment> UpdateAppointment(string appointmentId,string userId, AppointmentRequest request)
        {
            if(string.IsNullOrEmpty(appointmentId))
                throw new ArgumentNullException(nameof(appointmentId), "Appointment Id cant be null or empty.");
            if (request is null)
                throw new ArgumentNullException(nameof(appointmentId), "Appointment cant be null.");
            return await _appointmentStore.UpdateAppointment(appointmentId, userId, request);
        }
    }
}