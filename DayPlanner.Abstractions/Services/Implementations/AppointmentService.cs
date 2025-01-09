using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Abstractions.Services.Implementations;

public class AppointmentsService(IAppointmentStore appointmentStore) : IAppointmentsService
{
    private readonly IAppointmentStore _appointmentStore = appointmentStore;

    public async Task<Appointment> CreateAppointment(string userId, AppointmentRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(request);

        return await _appointmentStore.CreateAppointment(userId, request);
    }

    public async Task ImportOrUpdateAppointments(string userId, List<Appointment> appointments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(appointments);

        foreach (var appointment in appointments)
        {
            if (await _appointmentStore.GetAppointmentById(userId, appointment.Id) is not null)
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
                await _appointmentStore.ImportAppointment(userId, appointment.Id, new AppointmentRequest
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
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(appointmentId);

        await _appointmentStore.DeleteAppointment(userId, appointmentId);
    }

    public async Task<Appointment?> GetAppointmentById(string userId, string appointmendId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(appointmendId);

        return await _appointmentStore.GetAppointmentById(userId, appointmendId);
    }

    public async Task<long?> GetAppointmentsCount(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return await _appointmentStore.GetAppointmentsCount(userId);
    }

    public async Task<IEnumerable<Appointment>> GetUsersAppointments(string userId, DateTime start, DateTime end)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        if (start > end)
            throw new ArgumentException("Start date cant be greater than end date.");

        return await _appointmentStore.GetUsersAppointments(userId, start, end);
    }

    public async Task<IEnumerable<Appointment>> GetUsersAppointments(string userId, int page, int pageSize)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return await _appointmentStore.GetUsersAppointments(userId, page, pageSize);
    }

    public async Task<Appointment> UpdateAppointment(string appointmentId, string userId, AppointmentRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appointmentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(request);

        return await _appointmentStore.UpdateAppointment(appointmentId, userId, request);
    }
}