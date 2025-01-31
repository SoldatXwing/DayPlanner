using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using Refit;

namespace DayPlanner.Web.Refit;

[Headers("accept: application/json",
         "Authorization: Bearer")]
internal interface IDayPlannerApi
{
    #region Appointments
    /// <summary>
    /// Retrieves all appointments of the signed in user using pagination.
    /// </summary>
    /// <param name="page">The page to return.</param>
    /// <param name="pageSize">The maximum size of a page.</param>
    /// <returns>The paginated response including the items of the page.</returns>
    [Get("/appointments")]
    Task<PaginatedResponse<Appointment>> GetAllAppointmentsAsync([Query] int page = 0, [Query] int pageSize = 10);

    /// <summary>
    /// Retrieves all appointments that fit into a certain time range.
    /// </summary>
    /// <remarks>
    /// <paramref name="end"/> has to be greater than <paramref name="start"/>.
    /// </remarks>
    /// <param name="start">The start of the range.</param>
    /// <param name="end">The end of the range.</param>
    /// <returns>The appointment inside the range.</returns>
    [Get("/appointments/range")]
    Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync([Query] DateTime start, [Query] DateTime end);

    /// <summary>
    /// Returns the appointment a certain id belongs to.
    /// </summary>
    /// <param name="appointmentId">The id of the appointment.</param>
    /// <returns>The appointment.</returns>
    [Get("/appointments/{appointmentId}")]
    Task<Appointment?> GetAppointmentByIdAsync(string appointmentId);

    /// <summary>
    /// Creates a new appointment.
    /// </summary>
    /// <param name="request">The create appointment request.</param>
    /// <returns>The created appointment.</returns>
    [Post("/appointments")]
    Task<Appointment> CreateAppointmentAsync([Body] AppointmentRequest request);

    /// <summary>
    /// Updates an existing appointment.
    /// </summary>
    /// <param name="appointmentId">The id of the appointment to update.</param>
    /// <param name="request">The update appointment request.</param>
    /// <returns>The updated appointment</returns>
    [Put("/appointments/{appointmentId}")]
    Task<Appointment> UpdateAppointmentAsync(string appointmentId, [Body] AppointmentRequest request);

    /// <summary>
    /// Removes a certain appointment irrevocable.
    /// </summary>
    /// <param name="appointmentId">The ID of the appointment to remove.</param>
    /// <returns>A task to await the asynchronous operation.</returns>
    [Delete("/appointments/{appointmentId}")]
    Task DeleteAppointmentAsync(string appointmentId);
    #endregion

    #region GoogleCalendar
    /// <summary>
    /// Triggers the OAuth login procedure using google calendar.
    /// </summary>
    /// <returns>The url where the UI </returns>
    [Get("/googlecalendar/login")]
    Task<string> GoogleLoginAsync();

    [Get("/googlecalendar/sync")]
    Task GoogleSyncAppointmentsAsync();
    #endregion
}