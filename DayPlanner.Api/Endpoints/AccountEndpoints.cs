using Carter;
using DayPlanner.Authorization.Exceptions;
using DayPlanner.Authorization.Interfaces;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using static DayPlanner.Api.Models.AccountEndpoints;

namespace DayPlanner.Api.Endpoints
{
    public partial class AccountEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var noAuthGroup = app.MapGroup("none");
            noAuthGroup.MapPost("/Validate", ValidateToken);
            noAuthGroup.MapPost("/Register", RegisterUser);
            noAuthGroup.MapPost("/Login", Login);

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
        private async Task<IResult> RegisterUser([FromBody] RegisterUserRequest request,IAuthService authService)
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

                var userRecord = await authService.CreateUserAsync(userRecordArgs);
                return Results.Ok(new { Message = "User registered successfully.", Uid = userRecord.Uid });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Message = "Failed to register user.", Error = ex.Message });
            }
        }
    }
}
