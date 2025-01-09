namespace DayPlanner.Abstractions.Services;

public interface IAuthService
{
    /// <summary>
    /// Verifies an auth token.
    /// </summary>
    /// <param name="idToken">The token to validate.</param>
    /// <returns>The user id this auth token belongs to. If <c>null</c> the token were invalid.</returns>
    Task<string?> VerifyTokenAsync(string idToken);
}