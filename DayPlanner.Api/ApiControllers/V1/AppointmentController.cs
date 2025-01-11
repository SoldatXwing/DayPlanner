using Asp.Versioning;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Services;
using DayPlanner.Api.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DayPlanner.Api.ApiControllers.V1;

/// <summary>
/// Manages appointments of a user.
/// </summary>
[ApiController]
[ApiVersion(1)]
[Route("v{version:apiVersion}/appointments")]
public sealed class AppointmentController(IAppointmentsService appointmentService, ILogger<AppointmentController> logger) : ControllerBase
{
    private ILogger<AppointmentController> _Logger { get; } = logger;

    /// <summary>
    /// Get all appointments of the signed in user.
    /// </summary>
    /// <param name="page">The item page to return.</param>
    /// <param name="pageSize">The amount of items to return per page.</param>
    /// <response code="200">Success - Returns a collection with the appointments</response>
    [HttpGet]
    [ProducesResponseType<PaginatedResponse<Appointment>>(200)]
    public async Task<IActionResult> GetAllAppointmentsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        string userId = HttpContext.User.GetUserId()!;
        try
        {
            long? totalItems = await appointmentService.GetAppointmentsCount(userId);
            IEnumerable<Appointment> appointments = await appointmentService.GetUsersAppointments(userId, page, pageSize);

            return Ok(new PaginatedResponse<Appointment>(appointments, totalItems, page, pageSize));
        }
        catch (UnauthorizedAccessException)
        {
            _Logger.LogWarning($"Unauthorized access attempt by user with uid {userId}");
            return Unauthorized();
        }

    }

    /// <summary>
    /// Get all appointments of the signed in user that are inside a start and end date.
    /// </summary>
    /// <remarks>
    /// The <paramref name="end"/> have to be larger than <paramref name="start"/>.
    /// </remarks>
    /// <param name="start">The start date.</param>
    /// <param name="end">The end date.</param>
    /// <response code="200">Success - Returns a collection with the appointments</response>
    /// <response code="401"></response>
    [HttpGet("range")]
    [ProducesResponseType<Appointment[]>(200)]
    public async Task<IActionResult> GetAppointmentsByDateAsync([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        if (start > end)
        {
            _Logger.LogWarning("Invalid date times: Start date cant be greater than end date.");
            return BadRequest(new ApiErrorModel { Message = "Invalid date times", Error = "Start date cant be greater than end date." });
        }
        var userId = HttpContext.User.GetUserId()!;
        try
        {
            return Ok(await appointmentService.GetUsersAppointments(userId, start, end));
        }
        catch (UnauthorizedAccessException)
        {
            _Logger.LogWarning($"Unauthorized access attempt by user with uid {userId}");
            return Forbid();
        }
    }

    /// <summary>
    /// Creates a new appointment for the signed in user.
    /// </summary>
    /// <param name="request">The appointment to create.</param>
    /// <response code="201">Success - Returns the created appointment</response>
    [HttpPost]
    [ProducesResponseType<Appointment>(201)]
    public async Task<IActionResult> CreateAppointmentAsync([FromBody] AppointmentRequest request)
    {
        if (string.IsNullOrEmpty(request.Title) ||
                request.Start == default ||
                request.End == default)
        {
            _Logger.LogWarning("Invalid request attributes: At least one attribute is not valid.");
            return BadRequest(new ApiErrorModel { Message = "Invalid request attributes", Error = "At least one attribute is not valid." });
        }
        var userId = HttpContext.User.GetUserId()!;
        Appointment appointment = await appointmentService.CreateAppointment(userId, request);
        _Logger.LogInformation($"Appointment with id {appointment.Id} created for user with uid {userId}");
        return Created($"/v1/appointments/{appointment.Id}", appointment);
    }

    /// <summary>
    /// Retrieves an appointment with a certain id.
    /// </summary>
    /// <param name="appointmentId">The id of the appointment to get.</param>
    /// <response code="200">Success - Returns the appointment with the id</response>
    /// <response code="404">Not found - No appointment with the id was found</response>
    [HttpGet("{appointmentId}")]
    [ProducesResponseType<Appointment>(200)]
    [ProducesResponseType<ApiErrorModel>(404)]
    public async Task<IActionResult> GetAppointmentAsync([FromRoute] string appointmentId)
    {
        var userId = HttpContext.User.GetUserId()!;
        Appointment? appointment = await appointmentService.GetAppointmentById(userId, appointmentId);
        if (appointment is null)
        {
            _Logger.LogWarning($"Appointment with id {appointmentId} not found for user with uid {userId}");
            return AppointmentNotFound(appointmentId);
        }
        return Ok(appointment);
    }

    /// <summary>
    /// Updates an appointment of a user.
    /// </summary>
    /// <remarks>
    /// The whole appointment except of the id will be updated so values that shouldn't be changed must be set to their current value.
    /// </remarks>
    /// <param name="appointmentId">The id of the appointment to update.</param>
    /// <param name="request">The updated appointment.</param>
    /// <response code="200">Success - Returns the updated appointment</response>
    /// <response code="403">Forbidden - The signed in user doesn't have access to this appointment</response>
    [HttpPut("{appointmentId}")]
    [ProducesResponseType<Appointment>(200)]
    public async Task<IActionResult> UpdateAppointmentAsync([FromRoute] string appointmentId, [FromBody] AppointmentRequest request)
    {
        var userId = HttpContext.User.GetUserId()!;
        try
        {
            Appointment appointment = await appointmentService.UpdateAppointment(appointmentId, userId, request);
            _Logger.LogInformation($"Appointment with id {appointmentId} updated for user with uid {userId}");
            return Ok(appointment);
        }
        catch (UnauthorizedAccessException)
        {
            _Logger.LogWarning($"Unauthorized access attempt by user with uid {userId}");
            return Forbid();
        }
    }

    /// <summary>
    /// Removes an appointment of a user.
    /// </summary>
    /// <param name="appointmentId">The if of the appointment to remove.</param>
    /// <response code="204">Success - The appointment were successfully removed</response>
    /// <response code="403">Forbidden - The signed in user doesn't have access to this appointment</response>
    /// <response code="404">Not found - No appointment with the id was found</response>
    [HttpDelete("{appointmentId}")]
    [ProducesResponseType<ApiErrorModel>(404)]
    public async Task<IActionResult> DeleteAppointmentAsync([FromRoute] string appointmentId)
    {
        var userId = HttpContext.User.GetUserId()!;
        try
        {
            await appointmentService.DeleteUsersAppointment(userId, appointmentId);
            _Logger.LogInformation($"Appointment with id {appointmentId} deleted for user with uid {userId}");
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            _Logger.LogWarning($"Unauthorized access attempt by user with uid {userId}");
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _Logger.LogWarning($"Appointment with id {appointmentId} not found for user with uid {userId}");
            return NotFound(new ApiErrorModel { Message = "Appointment not found", Error = ex.Message });
        }
    }

    private NotFoundObjectResult AppointmentNotFound(string id) => NotFound(new ApiErrorModel { Message = "Appointment not found", Error = $"No appointment with id {id} found." });
}