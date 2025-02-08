using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;

namespace DayPlanner.Web.Services
{
    /// <summary>
    /// Service to retrieve the users appointments
    /// </summary>
    internal interface IAppointmentService
    {
        /// <summary>
        /// Return all appointments, that are in range
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <returns>List of appointments</returns>
        public Task<List<Appointment>> GetAllAppointmentsInRangeAsync(DateTime start, DateTime end);
        /// <summary>
        /// Create a new appointment
        /// </summary>
        /// <param name="request">Request dto</param>
        /// <returns>The created appointment</returns>
        public Task<Appointment> CreateAppointmentAsync(AppointmentRequest request);
        /// <summary>
        /// Update an existing appointment
        /// </summary>
        /// <param name="request">Request dto</param>
        /// <returns>The updated appointment</returns>
        public Task<Appointment> UpdateAppointmentAsync(string appointmentId,AppointmentRequest request);
        /// <summary>
        /// Delete an appointment
        /// </summary>
        /// <param name="appointmentId">Id of the appointment</param>
        /// <returns></returns>
        public Task DeleteAppointmentAsync(string appointmentId);
    }
}
