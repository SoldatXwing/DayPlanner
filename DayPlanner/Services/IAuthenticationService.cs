using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Services;

internal interface IAuthenticationService
{
    /// <summary>
    /// Tries to sign in a user.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <returns>The signed in user. If <c>null</c> the credentials were invalid.</returns>
    public Task<User?> LoginAsync(UserRequest request);

    /// <summary>
    /// Tries to logout a currently logged in user.
    /// </summary>
    /// <returns>A task to await this operatopn.</returns>
    public Task LogoutAsync();

    /// <summary>
    /// Tries to register a new user.
    /// </summary>
    /// <remarks>
    /// After the registration were successfully completed a sign in will be done.
    /// </remarks>
    /// <param name="request">The register request</param>
    /// <returns>If the registration is successful <c>user</c> is not null. If an error occurred while the registration <c>error</c> contains the returned error.</returns>
    public Task<(User? user, ApiErrorModel? error)> RegisterAsync(RegisterUserRequest request);
    /// <summary>
    /// Returns the auth url where the user can authenticate himself via google
    /// </summary>
    /// <returns>The url</returns>
    public Task<string> GetGoogleAuthUrlAsync();
    /// <summary>
    /// Tries to sign in a user via google.
    /// </summary>
    /// <param name="token">Token provided from google</param>
    /// <returns>The signed in user. If <c>null</c> the credentials were invalid.</returns>
    public Task<(User? user, ApiErrorModel? error)> LoginViaGoogleAsync(string token);
    /// <summary>
    /// Checks if a user is currently logged in.
    /// </summary>
    /// <returns>True if logged in, otherwise false</returns>
    Task<bool> IsLoggedInAsync();

}
