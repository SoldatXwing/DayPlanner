using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Authentication;
using DayPlanner.Extensions;
using DayPlanner.Refit;
using Refit;
using System.Net;

namespace DayPlanner.Services.Implementations;

internal class AuthenticationService(IDayPlannerApi api, AuthProvider authenticationProvider) : IAuthenticationService
{
    public async Task<User?> LoginAsync(UserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string authToken;
        try
        {
            authToken = await api.LoginAsync(request);
            authToken = authToken.Trim('"');     // Idk why but when the request is send using Refit a single " char is leading and trailing in front of the payload.
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            return null;
        }

        User user = await api.GetCurrentUserAsync(authToken);
        await authenticationProvider.SetUserAsync(user.ToClaimsPrincipial(authToken));
        return user;
    }

    public async Task<(User? user, ApiErrorModel? error)> RegisterAsync(RegisterUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        User user;
        string authToken;
        try
        {
            user = await api.RegisterUserAsync(request);
            authToken = await api.LoginAsync(new() { Email = request.Email!, Password = request.Password });
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            ApiErrorModel? errorModel = await ex.GetContentAsAsync<ApiErrorModel>();
            return (null, errorModel);
        }

        await authenticationProvider.SetUserAsync(user.ToClaimsPrincipial(authToken));
        return (user, null);
    }
}
