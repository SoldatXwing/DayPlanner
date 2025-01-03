using Carter;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Services;

namespace DayPlanner.Api.Endpoints
{
    public class AppointmentEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var authGroup = app.MapGroup("Appointment").RequireAuthorization();
            authGroup.MapGet("", GetAllAppointments);
            authGroup.MapPost("", CreateAppointment);
            authGroup.MapGet("/Range", GetAllAppointmentsByDate);
            authGroup.MapGet("/{appointmentId}", GetAppointment);
            authGroup.MapDelete("/{appointmentId}", DeleteAppointment);
            authGroup.MapPut("/{appointmentId}", UpdateAppointment);

        }
        private async Task<IResult> GetAllAppointments(
            IAppointmentsService appointmentService,
            HttpContext httpContext,
            int page = 1,
            int pageSize = 10)
        {
            var userId = httpContext.User.Claims.SingleOrDefault(c => c.Type == "user_id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();
            try
            {
                var totalItems = await appointmentService.GetAppointmentsCount(userId);
                var appointments = await appointmentService.GetUsersAppointments(userId, page, pageSize);

                var response = new PaginatedResponse<Appointment>(
                    appointments, totalItems, page, pageSize
                );

                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Unauthorized();
            }

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
                return Results.BadRequest(new ApiErrorModel { Message = "Invalid request attributes", Error = "Atlest one attribute is not valid." });
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

            var appointment = await appointmentService.GetAppointmentById(userId, appointmentId);
            return appointment is null ? Results.NotFound(new ApiErrorModel { Message = "Appointment not found", Error = $"No appointment with id {appointmentId} found." }) : Results.Ok(appointment);
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

            if (start > end)
                return Results.BadRequest(new ApiErrorModel { Message = "Invalid date times", Error = "Start date cant be greater than end date." });
            try
            {
                var appointments = await appointmentService.GetUsersAppointments(userId, start, end);
                return Results.Ok(appointments);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Unauthorized();
            }
           
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
                return Results.NotFound(new ApiErrorModel { Message = "Appointment not found", Error = ex.Message });
            }
        }
        private static async Task<IResult> UpdateAppointment(
            string appointmentId,
            AppointmentRequest request,
            IAppointmentsService appointmentService,
            HttpContext httpContext)
        {
            var userId = httpContext.User.Claims.Single(c => c.Type == "user_id").Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();
            try
            {
                var appointment = await appointmentService.UpdateAppointment(appointmentId, userId, request);
                return Results.Ok(appointment);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Unauthorized();
            }
            
        }
    }
}
