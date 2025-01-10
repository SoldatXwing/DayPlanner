using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace DayPlanner.Authentication;

public class LocalStorageAuthenticationProvider(ILocalStorageService localStorage) : AuthenticationStateProvider
{
    public const string StorageKey = "authenticationState";

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        ClaimsPrincipal? claimsPrincipal = await localStorage.GetItemAsync<ClaimsPrincipal>(StorageKey);
        return new(claimsPrincipal ?? new());
    }

    public async Task SetUserAsync(ClaimsPrincipal? user)
    {
        if (user is null)
        {
            await localStorage.RemoveItemAsync(StorageKey);
        }
        else
        {
            await localStorage.SetItemAsync(StorageKey, user);
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user ?? new())));
    }
}