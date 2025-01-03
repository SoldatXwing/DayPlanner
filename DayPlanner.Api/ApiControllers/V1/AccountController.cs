using Asp.Versioning;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Services;
using DayPlanner.Api.Extensions;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DayPlanner.Api.ApiControllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("v{version:apiVersion}/account")]
public sealed class AccountController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAccountInformationAsync([FromServices] IUserService userService)
    {
        string userId = HttpContext.User.GetUserId()!;
        UserRecord user = await userService.GetUserByIdAsync(userId);
        if (user is null)
            return NotFound(new ApiErrorModel { Error = $"User with uid {userId} not found.", Message = "User not found" });

        return Ok(user.ProviderData);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] UserRequest request, [FromServices] IJwtProvider jwtProvider)
    {
        try
        {
            var token = await jwtProvider.GetForCredentialsAsync(request.Email, request.Password);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorModel { Error = ex.Message, Message = "User not found" });
        }
    }

    [AllowAnonymous]
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateTokenAsync([FromHeader] string authorization, [FromServices] IAuthService authService)
    {
        string? token = authorization?.Split(" ").Last();
        if (string.IsNullOrEmpty(token))
            return Unauthorized();

        try
        {
            FirebaseToken firebaseToken = await authService.VerifyTokenAsync(token);
            return Ok(new { firebaseToken.Uid });
        }
        catch
        {
            return Unauthorized();
        }
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUserRequest request, [FromServices] IUserService userService)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest("Email and password are required.");

        try
        {
            var userRecordArgs = new UserRecordArgs
            {
                Email = request.Email,
                Password = request.Password,
                DisplayName = request.DisplayName
            };

            UserRecord userRecord = await userService.CreateUserAsync(userRecordArgs);
            return Ok(new { Message = "User registered successfully.", userRecord.Uid });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorModel { Message = "Failed to register user.", Error = ex.Message });
        }
    }
}
