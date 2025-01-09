using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;

namespace DayPlanner.Abstractions.Services;

public interface IAppointmentsService
{
    Task<IEnumerable<Appointment>> GetUsersAppointments(string userId, DateTime start, DateTime end);

    Task<long?> GetAppointmentsCount(string userId);

    Task<IEnumerable<Appointment>> GetUsersAppointments(string userId, int page, int pageSize);

    Task DeleteUsersAppointment(string userId, string appointmentId);

    Task<Appointment?> GetAppointmentById(string userId, string appointmendId);

    Task<Appointment> UpdateAppointment(string appointmentId, string userId, AppointmentRequest request);

    Task<Appointment> CreateAppointment(string userId, AppointmentRequest request);

    Task ImportOrUpdateAppointments(string userId, List<Appointment> appointments);
}