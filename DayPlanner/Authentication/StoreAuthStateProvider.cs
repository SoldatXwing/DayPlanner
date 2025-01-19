using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Extensions;
using DayPlanner.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace DayPlanner.Authentication;

internal class StoreAuthStateProvider(IPersistentStore store) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        (User user, string _)? userData = await store.GetUserAsync();

        ClaimsPrincipal principal = userData?.user.ToClaimsPrincipial() ?? new();
        return new(principal);
    }

    /// <summary>
    /// Notifies all listener that the authentication state may have changed.
    /// </summary>
    public void NotifyUserChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}