using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Web.Extensions;
using DayPlanner.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace DayPlanner.Web.Components.Pages.Account;

[AllowAnonymous]
[Route("/account/google/login")]
public sealed class GoogleLogin : ComponentBase
{
    #region Injections
    [Inject]
    private Services.IAuthenticationService AuthenticationService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    #endregion

    [SupplyParameterFromQuery(Name = "token")]
    private string? Token { get; set; }

    [SupplyParameterFromQuery(Name = "refreshToken")]
    private string? RefreshToken { get; set; }

    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (HttpContext is null)
        {
            NavigationManager.Refresh(forceReload: true);
            return;
        }

        if (string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(RefreshToken))
        {
            NavigationManager.NavigateToLogin();
            return;
        }

        (UserSession? session, ApiErrorModel? _) = await AuthenticationService.LoginViaGoogleAsync(Token);
        if (session is null)
        {
            NavigationManager.NavigateToLogin();
        }
        else
        {
            await HttpContext.SignInAsync(session.ToClaimsPrincipial());
            NavigationManager.NavigateToDashboard();
        }
    }
}
