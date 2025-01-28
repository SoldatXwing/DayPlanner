using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Authentication;
using DayPlanner.Models;
using DayPlanner.Refit;
using Newtonsoft.Json;
using Refit;
using System.Net;

namespace DayPlanner.Services.Implementations;

internal class DefaultAuthenticationService(IDayPlannerAccountApi api, IPersistentStore store, StoreAuthStateProvider authStateProvider) : IAuthenticationService
{
    public async Task<User?> LoginAsync(UserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        TokenResponse tokenData;
        try
        {
            tokenData = JsonConvert.DeserializeObject<TokenResponse>(await api.LoginAsync(request))!;
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            return null;
        }

        User user = await api.GetCurrentUserAsync(tokenData.Token);
        await store.SetUserAsync(user, tokenData.Token);
        authStateProvider.NotifyUserChanged();

        return user;
    }
    public async Task<string> GetGoogleAuthUrlAsync()
    {
        try
        {
            return await api.GetGoogleAuthUrl();
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            return null!;
        }
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

    public async Task<(User? user, ApiErrorModel? error)> LoginViaGoogleAsync(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        try
        {
            User user = await api.GetCurrentUserAsync(token);
            await store.SetUserAsync(user, token);
            authStateProvider.NotifyUserChanged();

            return (user, null);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            ApiErrorModel? errorModel = await ex.GetContentAsAsync<ApiErrorModel>();
            return (null, errorModel);
        }

    }

    public async Task<bool> IsLoggedInAsync()
    {
        var user = await store.GetUserAsync();        
        return user != null;
    }
}
