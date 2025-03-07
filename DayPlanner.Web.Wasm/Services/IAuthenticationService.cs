using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Wasm.Models;

namespace DayPlanner.Web.Wasm.Services
{
    internal interface IAuthenticationService
    {
        /// <summary>
        /// Tries to sign in a user.
        /// </summary>
        /// <param name="request">The login request.</param>
        /// <returns>The signed in user. If <c>null</c> the credentials were invalid.</returns>
        public Task<UserSession?> LoginAsync(UserRequest request);

        /// <summary>
        /// Tries to register a new user.
        /// </summary>
        /// <remarks>
        /// After the registration were successfully completed a sign in will be done.
        /// </remarks>
        /// <param name="request">The register request</param>
        /// <returns>If the registration is successful <c>user</c> is not null. If an error occurred while the registration <c>error</c> contains the returned error.</returns>
        public Task<(UserSession? user, ApiErrorModel? error)> RegisterAsync(RegisterUserRequest request);
        /// <summary>
        /// Returns the auth url where the user can authenticate himself via google
        /// </summary>
        /// <returns>The url</returns>
        public Task<string> GetGoogleAuthUrlAsync();
        /// <summary>
        /// Tries to sign in a user via google.
        /// </summary>
        /// <param name="token">Token provided from google</param>
        /// <param name="refreshToken">Refreshtoken provided from google</param>
        /// <returns>The signed in user. If <c>null</c> the credentials were invalid.</returns>
        public Task<(UserSession? user, ApiErrorModel? error)> LoginViaGoogleAsync(string token, string refreshToken);

    }
}
