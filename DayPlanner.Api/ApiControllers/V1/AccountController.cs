﻿using Asp.Versioning;
using DayPlanner.Abstractions.Exceptions;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Services;
using DayPlanner.Api.Extensions;
using DayPlanner.Authorization.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace DayPlanner.Api.ApiControllers.V1;

/// <summary>
/// Manages user login and registration.
/// </summary>
[ApiController]
[ApiVersion(1)]
[Route("v{version:apiVersion}/account")]
public sealed partial class AccountController(ILogger<AccountController> logger) : ControllerBase
{
    /// <summary>
    /// Gets the logger instance for this controller
    /// </summary>
    private ILogger<AccountController> _Logger { get; } = logger;

    /// <summary>
    /// Gets the login provider data associated with the user of the current session.
    /// </summary>
    /// <response code="200">Success - Returns the user</response>
    /// <response code="404">Not found - The user were internally not found</response>
    [HttpGet]
    [ProducesResponseType<User>(200)]
    [ProducesResponseType<ApiErrorModel>(404)]
    public async Task<IActionResult> GetAccountInformationAsync([FromServices] IUserService userService)
    {
        string userId = HttpContext.User.GetUserId()!;
        User? user = await userService.GetUserByIdAsync(userId);
        if (user is null)
        {
            _Logger.LogWarning("User with uid {UserId} not found.", userId);
            return NotFound(new ApiErrorModel { Error = $"User with uid {userId} not found.", Message = "User not found" });
        }

        return Ok(user);
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
        if(string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            _Logger.LogWarning("Invalid data. Email and password are required.");
            return BadRequest(new ApiErrorModel { Error = "Invalid data", Message = "Email and password are required." });
        }
        try
        {
            string token = await jwtProvider.GetForCredentialsAsync(request.Email, request.Password);
            return Ok(token);
        }
        catch (Exception ex) when (ex.GetType() == typeof(BadCredentialsException) || ex.GetType() == typeof(InvalidEmailException))
        {
            _Logger.LogWarning("Invalid email or password provided for login attempt. Email: {Email}", request.Email);
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
        if (string.IsNullOrEmpty(authorization))
        {
            _Logger.LogWarning("No authorization header provided.");
            return Unauthorized();
        }
        string? token = authorization?.Split(" ").Last();
        if (string.IsNullOrEmpty(token))
        {
            _Logger.LogWarning("No token provided.");
            return Unauthorized();
        }
        string? userId = await authService.VerifyTokenAsync(token);
        return !string.IsNullOrEmpty(userId)
            ? Ok(userId)
            : Unauthorized();
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">Provided data for the new user.</param>
    /// <param name="userService"></param>
    /// <response code="200">Success - Returns the created user</response>
    /// <response code="400">Bad request - Invalid data were provided</response>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<User>(200)]
    [ProducesResponseType<ApiErrorModel>(400)]
    public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUserRequest request, [FromServices] IUserService userService)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            _Logger.LogWarning("Invalid data. Email and password are required.");
            return BadRequest(new ApiErrorModel { Error = "Invalid data", Message = "Email and password are required." });
        }
        if (request.Password.Length < 6)
        {
            _Logger.LogWarning("Password is less than 6 characters long.");
            return BadRequest(new ApiErrorModel { Message = "Invalid password", Error = "Password must be at least 6 characters long." });
        }
        if (!ValidEmail().Match(request.Email).Success)
        {
            _Logger.LogWarning("Invalid email provided: {Email}", request.Email);
            return BadRequest(new ApiErrorModel { Message = "Invalid email", Error = "Invalid email provided." });
        }
        if (request.PhoneNumber is not null && !ValidPhoneNumber().Match(request.PhoneNumber).Success)
        {
            _Logger.LogWarning("Invalid phone number provided: {PhoneNumber}", request.PhoneNumber);
            return BadRequest(new ApiErrorModel { Message = "Invalid phone number", Error = "Invalid phone number provided." });
        }
        try
        {
            User userRecord = await userService.CreateUserAsync(request);
            return Ok(userRecord);
        }
        catch (InvalidOperationException ex)
            when (ex.Message == "Email already in use")
        {
            _Logger.LogWarning("Email {Email} is already in use.", request.Email);
            return BadRequest(new ApiErrorModel { Message = "Invalid Email", Error = "Email is already in use" });
        }
        catch (InvalidOperationException ex)
            when (ex.Message == "Phone number already in use")
        {
            _Logger.LogWarning("Phone number {PhoneNumber} is already in use.", request.PhoneNumber);
            return BadRequest(new ApiErrorModel { Message = "Invalid Phone Number", Error = "Phone number is already in use" });
        }
    }
    /// <summary>
    /// Validates a phone number.
    /// </summary>
    /// <returns>True if valid, otherwise false</returns>
    [GeneratedRegex(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$")]
    private static partial Regex ValidPhoneNumber();
    /// <summary>
    /// Validates an email address.
    /// </summary>
    /// <returns>True if valid, otherwise false</returns>
    [GeneratedRegex(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])")]
    private static partial Regex ValidEmail();

}
