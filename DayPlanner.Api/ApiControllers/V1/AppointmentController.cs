using Asp.Versioning;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Services;
using DayPlanner.Api.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using static Google.Rpc.Context.AttributeContext.Types;

namespace DayPlanner.Api.ApiControllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("v{version:apiVersion}/appointment")]
public sealed class AppointmentController(IAppointmentsService appointmentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAppointmentsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        string userId = HttpContext.User.GetUserId()!;
        try
        {
            long? totalItems = await appointmentService.GetAppointmentsCount(userId);
            List<Appointment> appointments = await appointmentService.GetUsersAppointments(userId, page, pageSize);

            return Ok(new PaginatedResponse<Appointment>(appointments, totalItems, page, pageSize));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpGet("range")]
    public async Task<IActionResult> GetAppointmentsByDateAsync([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        if (start > end)
            return BadRequest(new ApiErrorModel { Message = "Invalid date times", Error = "Start date cant be greater than end date." });
        try
        {
            var appointments = await appointmentService.GetUsersAppointments(HttpContext.User.GetUserId()!, start, end);
            return Ok(appointments);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointmentAsync([FromBody] AppointmentRequest request)
    {
        if (string.IsNullOrEmpty(request.Title) ||
                request.Start == default ||
                request.End == default)
        {
            return BadRequest(new ApiErrorModel { Message = "Invalid request attributes", Error = "At least one attribute is not valid." });
        }

        Appointment appointment = await appointmentService.CreateAppointment(HttpContext.User.GetUserId()!, request);
        return CreatedAtAction(
            actionName: nameof(GetAppointmentAsync),
            routeValues: new { appointmentId = appointment.Id },
            value: appointment);
    }

    [HttpGet("{appointmentId}")]
    public async Task<IActionResult> GetAppointmentAsync([FromRoute] string appointmentId)
    {
        Appointment? appointment = await appointmentService.GetAppointmentById(HttpContext.User.GetUserId()!, appointmentId);
        return appointment is null
            ? NotFound(new ApiErrorModel { Message = "Appointment not found", Error = $"No appointment with id {appointmentId} found." }) 
            : Ok(appointment);
    }

    [HttpPut("{appointmentId}")]
    public async Task<IActionResult> UpdateAppointmentAsync([FromRoute] string appointmentId, [FromBody] AppointmentRequest request)
    {
        try
        {
            var appointment = await appointmentService.UpdateAppointment(appointmentId, HttpContext.User.GetUserId()!, request);
            return Ok(appointment);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpDelete("{appointmentId}")]
    public async Task<IActionResult> DeleteAppointmentAsync([FromRoute] string appointmentId)
    {
        try
        {
            await appointmentService.DeleteUsersAppointment(HttpContext.User.GetUserId()!, appointmentId);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            //TODO: log
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ApiErrorModel { Message = "Appointment not found", Error = ex.Message });
        }
    }
}