using DayPlanner.Abstractions.Enums;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Abstractions.Services.Implementations;

/// <summary>
/// Service for managing appointments.
/// </summary>
/// <param name="appointmentStore">The appointment store for managing appointments data.</param>
public class AppointmentsService(IAppointmentStore appointmentStore) : IAppointmentsService
{
    private readonly IAppointmentStore _appointmentStore = appointmentStore;

    /// <summary>
    /// Creates an appointment for a user.
    /// </summary>
    /// <param name="userId">The ID of the user creating the appointment.</param>
    /// <param name="request">The details of the appointment to be created.</param>
    /// <returns>The created appointment.</returns>
    public async Task<Appointment> CreateAppointment(string userId, AppointmentRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(request);

        return await _appointmentStore.CreateAppointment(userId, request);
    }

    /// <summary>
    /// Imports or updates appointments for a user.
    /// </summary>
    /// <param name="userId">The ID of the user whose appointments are being managed.</param>
    /// <param name="appointments">The list of appointments to import or update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ImportOrUpdateAppointments(string userId, List<Appointment> appointments, CalendarOrigin calendarOrigin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(appointments);

        foreach (var appointment in appointments)
        {
            if (await _appointmentStore.GetAppointmentById(userId, appointment.Id) is not null)
            {
                await _appointmentStore.UpdateAppointment(appointment.Id, userId, new AppointmentRequest(calendarOrigin)
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
                await _appointmentStore.ImportAppointment(userId, appointment.Id, new AppointmentRequest(calendarOrigin)
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

    /// <summary>
    /// Deletes a user's appointment.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="appointmentId">The ID of the appointment to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteUsersAppointment(string userId, string appointmentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(appointmentId);

        await _appointmentStore.DeleteAppointment(userId, appointmentId);
    }

    /// <summary>
    /// Retrieves an appointment by its ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="appointmendId">The ID of the appointment.</param>
    /// <returns>The appointment if found, or null otherwise.</returns>
    public async Task<Appointment?> GetAppointmentById(string userId, string appointmendId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(appointmendId);

        return await _appointmentStore.GetAppointmentById(userId, appointmendId);
    }

    /// <summary>
    /// Retrieves the count of appointments for a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The count of appointments.</returns>
    public async Task<long?> GetAppointmentsCount(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return await _appointmentStore.GetAppointmentsCount(userId);
    }

    /// <summary>
    /// Retrieves a list of appointments for a user within a specific date range.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="start">The start date of the range.</param>
    /// <param name="end">The end date of the range.</param>
    /// <returns>A list of appointments within the specified date range.</returns>
    /// <exception cref="ArgumentException">Thrown when the start date is greater than the end date.</exception>
    public async Task<IEnumerable<Appointment>> GetUsersAppointments(string userId, DateTime start, DateTime end)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        if (start > end)
            throw new ArgumentException("Start date cant be greater than end date.");

        return await _appointmentStore.GetUsersAppointments(userId, start, end);
    }

    /// <summary>
    /// Retrieves a paginated list of appointments for a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The number of appointments per page.</param>
    /// <returns>A paginated list of appointments.</returns>
    public async Task<IEnumerable<Appointment>> GetUsersAppointments(string userId, int page, int pageSize)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return await _appointmentStore.GetUsersAppointments(userId, page, pageSize);
    }

    /// <summary>
    /// Updates an existing appointment.
    /// </summary>
    /// <param name="appointmentId">The ID of the appointment to update.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="request">The new details of the appointment.</param>
    /// <returns>The updated appointment.</returns>
    public async Task<Appointment> UpdateAppointment(string appointmentId, string userId, AppointmentRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appointmentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(request);

        return await _appointmentStore.UpdateAppointment(appointmentId, userId, request);
    }
    /// <summary>
    /// Deletes appointments by calendar origin.
    /// </summary>
    /// <param name="userId">Id of the user</param>
    /// <param name="origin">Calendar origin, which appointments should be deleted</param>
    /// <returns></returns>
    public async Task DeleteAppointmentsByCalendarOrigin(string userId, CalendarOrigin origin)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentNullException.ThrowIfNull(origin);
        await _appointmentStore.DeleteAppointmentsByCalendarOrigin(userId, origin);
    }
}