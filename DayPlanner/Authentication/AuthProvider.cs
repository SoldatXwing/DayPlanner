using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace DayPlanner.Authentication;

internal class AuthProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal? _user;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_user ?? new()));
    }

    public Task SetUserAsync(ClaimsPrincipal? user)
    {
        _user = user;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user ?? new())));

        return Task.CompletedTask;
    }
}