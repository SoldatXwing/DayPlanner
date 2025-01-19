using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Authentication;
using DayPlanner.Refit;
using Refit;
using System.Net;

namespace DayPlanner.Services.Implementations;

internal class DefaultAuthenticationService(IDayPlannerAccountApi api, IPersistentStore store, StoreAuthStateProvider authStateProvider) : IAuthenticationService
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
        await store.SetUserAsync(user, authToken);
        authStateProvider.NotifyUserChanged();

        return user;
    }

    public async Task LogoutAsync()
    {
        await store.RemoveUserAsync();
        authStateProvider.NotifyUserChanged();
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

        await store.SetUserAsync(user, authToken);
        authStateProvider.NotifyUserChanged();

        return (user, null);
    }
}
