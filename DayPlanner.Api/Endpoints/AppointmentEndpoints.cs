using Carter;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DayPlanner.Api.Endpoints
{
    public class AppointmentEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var authGroup = app.MapGroup("Appointment").RequireAuthorization();
            authGroup.MapGet("", GetAllAppointments);
            authGroup.MapPost("", CreateAppointment);
            authGroup.MapGet("/{appointmentId}", GetAppointment);
            authGroup.MapGet("/Range", GetAllAppointmentsByDate);

        }
        private async Task<IResult> GetAllAppointments(
            IAppointmentsService appointmentService,
            HttpContext httpContext)
        {
            var userId = httpContext.User.Claims.Single(c => c.Type == "user_id").Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var appointments = await appointmentService.GetUsersAppointments(userId);
            return Results.Ok(appointments);
        }
        private async Task<IResult> CreateAppointment(
            AppointmentRequest request,
            IAppointmentsService appointmentService,
            HttpContext httpContext)
        {
            if (string.IsNullOrEmpty(request.Title) ||
                request.Start == default ||
                request.End == default)
            {
                return Results.BadRequest(new { Message = "Invalid request attributes", Error ="Atlest one attribute is not valid."});
            }
            var userId = httpContext.User.Claims.Single(c => c.Type == "user_id").Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var appointment = await appointmentService.CreateAppointment(userId, request);
            return Results.Created(nameof(GetAppointment), appointment);
        }
        private static async Task<IResult> GetAppointment(
            string appointmentId,
            IAppointmentsService appointmentService,
            HttpContext httpContext)
        {
            var userId = httpContext.User.Claims.Single(c => c.Type == "user_id").Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var appointment = await appointmentService.GetAppointmentById(appointmentId);
            return appointment is null ? Results.NotFound(new { Message = "Appointment not found",Error = $"No appointment with id {appointmentId} found."}) : Results.Ok(appointment);
        }
        private static async Task<IResult> GetAllAppointmentsByDate(
            string appointmentId,
            DateTime start,
            DateTime end,
            IAppointmentsService appointmentService,
            HttpContext httpContext)
        {
            var userId = httpContext.User.Claims.Single(c => c.Type == "user_id").Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            if(start > end)
                return Results.BadRequest(new { Message = "Invalid date times",Error = "Start date cant be greater than end date." });

            var appointments = await appointmentService.GetUsersAppointments(userId, start, end);
            return Results.Ok(appointments);
        }
        private static async Task<IResult> DeleteAppointment(
            string appointmentId,
            IAppointmentsService appointmentService,
            HttpContext httpContext)
        {
            var userId = httpContext.User.Claims.Single(c => c.Type == "user_id").Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();
            try
            {
                await appointmentService.DeleteUsersAppointment(userId, appointmentId);
                return Results.Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                //TODO: log
                return Results.Unauthorized();
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { Message = "Appointment not found", Error = ex.Message });
            }
        }
    }
}
