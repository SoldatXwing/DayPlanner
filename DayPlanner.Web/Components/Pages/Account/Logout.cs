using DayPlanner.Web.Services;
using Microsoft.AspNetCore.Components;

namespace DayPlanner.Web.Components.Pages.Account;

[Route("/account/logout")]
public sealed class Logout : ComponentBase
{
    #region Injections
    [Inject]
    private IAuthenticationService AuthenticationService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    #endregion

    protected override async Task OnInitializedAsync()
    {
        await AuthenticationService.LogoutAsync();
        NavigationManager.NavigateToLogin();
    }
}
