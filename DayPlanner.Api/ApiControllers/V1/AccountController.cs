using Asp.Versioning;
using DayPlanner.Abstractions.Exceptions;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Services;
using DayPlanner.Api.Extensions;
using DayPlanner.Authorization.Exceptions;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DayPlanner.Api.ApiControllers.V1;

/// <summary>
/// Manages user login and registration.
/// </summary>
[ApiController]
[ApiVersion(1)]
[Route("v{version:apiVersion}/account")]
public sealed class AccountController : ControllerBase
{
    /// <summary>
    /// Gets the login provider data associated with the user of the current session.
    /// </summary>
    /// <response code="200">Success - Returns a collection of provider data associated with the user</response>
    /// <response code="404">Not found - The user were internally not found</response>
    [HttpGet]
    [ProducesResponseType<IUserInfo[]>(200)]
    [ProducesResponseType<ApiErrorModel>(404)]
    public async Task<IActionResult> GetAccountInformationAsync([FromServices] IUserService userService)
    {
        string userId = HttpContext.User.GetUserId()!;
        UserRecord user = await userService.GetUserByIdAsync(userId);
        if (user is null)
            return NotFound(new ApiErrorModel { Error = $"User with uid {userId} not found.", Message = "User not found" });

        return Ok(user.ProviderData);
    }

    /// <summary>
    /// Tries to sign in for a user.
    /// </summary>
    /// <param name="request">Provided data for the sign in.</param>
    /// <param name="jwtProvider"></param>
    /// <response code="200">Success - Returns the complete bearer access token</response>
    /// <response code="400">Bad request - Invalid email or password</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<ApiErrorModel>(400)]
    public async Task<IActionResult> LoginAsync([FromBody] UserRequest request, [FromServices] IJwtProvider jwtProvider)
    {
        try
        {
            string token = await jwtProvider.GetForCredentialsAsync(request.Email, request.Password);
            return Ok(token);
        }
        catch (Exception ex) when (ex.GetType() == typeof(BadCredentialsException)|| ex.GetType() == typeof(InvalidEmailException))
        {
            return BadRequest(new ApiErrorModel { Error = ex.Message, Message = "Invalid email or password." });
        }
    }

    /// <summary>
    /// Validates the current login token.
    /// </summary>
    /// <param name="authorization">The login token to verify.</param>
    /// <param name="authService"></param>
    /// <response code="200">Success - Returns the Uid of the signed in user</response>
    /// <response code="401">Unauthorized - The specified token is invalid and cannot be used</response>
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
            return Ok(firebaseToken.Uid);
        }
        catch
        {
            return Unauthorized();
        }
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">Provided data for the new user.</param>
    /// <param name="userService"></param>
    /// <response code="200">Success - Returns the Uid of the created user</response>
    /// <response code="400">Bad request - Invalid data were provided</response>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<ApiErrorModel>(400)]
    public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUserRequest request, [FromServices] IUserService userService)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new ApiErrorModel { Error = "Invalid data", Message = "Email and password are required." });

        try
        {
            UserRecordArgs userRecordArgs = new()
            {
                Email = request.Email,
                Password = request.Password,
                DisplayName = request.DisplayName
            };

            UserRecord userRecord = await userService.CreateUserAsync(userRecordArgs);
            return Ok(userRecord.Uid);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorModel { Message = "Failed to register user.", Error = ex.Message });
        }
    }
}
