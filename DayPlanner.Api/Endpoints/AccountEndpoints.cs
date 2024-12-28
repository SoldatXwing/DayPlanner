using Carter;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Services;
using DayPlanner.Authorization.Exceptions;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DayPlanner.Api.Endpoints
{
    public class AccountEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var noAuthGroup = app.MapGroup("Account");            
            noAuthGroup.MapPost("/Validate", ValidateToken);
            noAuthGroup.MapPost("/Register", RegisterUser);
            noAuthGroup.MapPost("/Login", Login);

            var authGroup = app.MapGroup("Account").RequireAuthorization();
            authGroup.MapGet("", GetAccountInformation);

        }
        private async Task<IResult> GetAccountInformation(HttpContext httpContext, IUserService userService)
        {
            var userId = httpContext.User.Claims.Single(c => c.Type == "user_id").Value; 
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized(); 


            var user = await userService.GetUserByIdAsync(userId);
            if (user is null)
                return Results.NotFound($"User with uid {userId} not found.");

            return Results.Ok(user.ProviderData);
        }
        private async Task<IResult> Login([FromBody] UserRequest request, IJwtProvider jwtProvider)
        {
            try
            {
                var token = await jwtProvider.GetForCredentialsAsync(request.Email, request.Password);
                return Results.Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });

            }

        }
        private async Task<IResult> ValidateToken([FromHeader] string authorization, IAuthService authService)
        {
            var token = authorization?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
                return Results.Unauthorized();

            try
            {
                var firebaseToken = await authService.VerifyTokenAsync(token);
                return Results.Ok(new { Uid = firebaseToken.Uid });
            }
            catch
            {
                return Results.Unauthorized();
            }
        }
        private async Task<IResult> RegisterUser([FromBody] RegisterUserRequest request,IUserService userService)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return Results.BadRequest("Email and password are required.");

            try
            {
                var userRecordArgs = new UserRecordArgs
                {
                    Email = request.Email,
                    Password = request.Password,
                    DisplayName = request.DisplayName
                };

                var userRecord = await userService.CreateUserAsync(userRecordArgs);
                return Results.Ok(new { Message = "User registered successfully.", Uid = userRecord.Uid });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Message = "Failed to register user.", Error = ex.Message });
            }
        }
    }
    public class AppointmentEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var authGroup = app.MapGroup("Appointment").RequireAuthorization();

        }
        private async Task<IResult> GetAllAppointments(IAppointmentsService appointmentService)
        {
            return Results.Ok();
        }
    }
}
