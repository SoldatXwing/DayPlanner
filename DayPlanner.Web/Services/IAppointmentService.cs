using DayPlanner.Abstractions.Models.Backend;

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
    }
}
