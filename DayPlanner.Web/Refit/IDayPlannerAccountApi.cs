    using DayPlanner.Abstractions.Models.Backend;
    using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Models;
using Refit;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace DayPlanner.Web.Refit;

    [Headers("accept: application/json")]
    internal interface IDayPlannerAccountApi
    {
        /// <summary>
        /// Returns the currently signed in user.
        /// </summary>
        /// <param name="token">The auth token to use for this request.</param>
        /// <returns>The signed in user.</returns>
        [Get("/account")]
        Task<User> GetCurrentUserAsync([Authorize] string token);

        /// <summary>
        /// Validates an auth token
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>The ID of the user the token belongs to.</returns>
        [Post("/account/validate")]
        Task<string> ValidateTokenAsync([Authorize] string token);

        /// <summary>
        /// Logs in into a user's account.
        /// </summary>
        /// <param name="request">The login request.</param>
        /// <returns>The auth token if successful.</returns>
        [Post("/account/login")]
        Task<TokenResponse> LoginAsync([Body] UserRequest request);

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="request">The register request.</param>
        /// <returns>The created user.</returns>
        [Post("/account/register")]
        Task<User> RegisterUserAsync([Body] RegisterUserRequest request);
        /// <summary>
        /// Return the url where the user can authenticate himself via google
        /// </summary>
        /// <returns>The Url</returns>
        [Get("/account/login/google")]
        Task<string> GetGoogleAuthUrl([Query] string os);
    }
