using DayPlanner.Abstractions.Enums;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;

namespace DayPlanner.Abstractions.Stores;

/// <summary>
/// Interface for interacting with appointment data storage, providing methods for 
/// retrieving, creating, updating, and deleting appointments for users.
/// </summary>
public interface IAppointmentStore
{
    /// <summary>
    /// Retrieves a list of appointments for a user within a specific date range.
    /// </summary>
    /// <param name="userId">The ID of the user whose appointments are to be fetched.</param>
    /// <param name="start">The start date and time of the range to filter appointments.</param>
    /// <param name="end">The end date and time of the range to filter appointments.</param>
    /// <returns>A list of <see cref="Appointment"/> objects representing the user's appointments.</returns>
    Task<IEnumerable<Appointment>> GetUsersAppointments(string userId, DateTime start, DateTime end);

    /// <summary>
    /// Retrieves a paginated list of appointments for a user.
    /// </summary>
    /// <param name="userId">The ID of the user whose appointments are to be fetched.</param>
    /// <param name="page">The page number for the paginated results.</param>
    /// <param name="pageSize">The number of appointments per page.</param>
    /// <returns>A list of <see cref="Appointment"/> objects representing the user's appointments on the specified page.</returns>
    Task<IEnumerable<Appointment>> GetUsersAppointments(string userId, int page, int pageSize);

    /// <summary>
    /// Deletes an appointment for a user by its ID.
    /// </summary>
    /// <param name="userId">The ID of the user who owns the appointment.</param>
    /// <param name="appointmentId">The ID of the appointment to be deleted.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAppointment(string userId, string appointmentId);

    /// <summary>
    /// Retrieves a specific appointment by its ID for a user.
    /// </summary>
    /// <param name="userId">The ID of the user who owns the appointment.</param>
    /// <param name="appointmentId">The ID of the appointment to be fetched.</param>
    /// <returns>The appointment object if found, or <c>null</c> if not found.</returns>
    Task<Appointment?> GetAppointmentById(string userId, string appointmentId);

    /// <summary>
    /// Updates an existing appointment with new details.
    /// </summary>
    /// <param name="appointmentId">The ID of the appointment to be updated.</param>
    /// <param name="userId">The ID of the user who owns the appointment.</param>
    /// <param name="request">The <see cref="AppointmentRequest"/> object containing the updated details.</param>
    /// <returns>The updated <see cref="Appointment"/> object.</returns>
    Task<Appointment> UpdateAppointment(string appointmentId, string userId, AppointmentRequest request);

    /// <summary>
    /// Creates a new appointment for a user.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the appointment is being created.</param>
    /// <param name="request">The <see cref="AppointmentRequest"/> object containing the appointment details.</param>
    /// <returns>The created <see cref="Appointment"/> object.</returns>
    Task<Appointment> CreateAppointment(string userId, AppointmentRequest request);

    /// <summary>
    /// Retrieves the count of appointments for a user.
    /// </summary>
    /// <param name="userId">The ID of the user whose appointment count is being requested.</param>
    /// <returns>The total number of appointments for the user, or <c>null</c> if the count cannot be determined.</returns>
    Task<long?> GetAppointmentsCount(string userId);

    /// <summary>
    /// Imports 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="externalId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<Appointment> ImportAppointment(string userId, string externalId, AppointmentRequest request);
    Task DeleteAppointmentsByCalendarOrigin(string userId, CalendarOrigin origin);

}